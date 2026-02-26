using Dapper;
using DMSMigration.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DMSMigration.Data.Repositories;

public class DapperDocumentRepository : IDapperDocumentRepository
{
    private readonly string _connectionString;
    private readonly ILogger<DapperDocumentRepository> _logger;

    public DapperDocumentRepository(IConfiguration configuration, ILogger<DapperDocumentRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection not found");
        _logger = logger;
    }

    private IDbConnection CreateConnection() => new OracleConnection(_connectionString);

    public async Task<int> InsertDocumentAsync(DmsDocument document)
    {
        const string sql = @"
            INSERT INTO DMS_DOCUMENT 
            (FILE_NAME, EXTENSION, ""PATH"", ""SIZE"", TYPE_ID, CURRENT_VERSION, CREATION_TIME, 
             CREATOR_USER_ID, LAST_MODIFICATION_TIME, LAST_MODIFIER_USER_ID, 
             IS_DELETED, DELETER_USER_ID, DELETION_TIME, TENANT_ID)
            VALUES 
            (:FileName, :Extension, :FilePath, :FileSize, :TypeId, :CurrentVersion, :CreationTime,
             :CreatorUserId, :LastModificationTime, :LastModifierUserId,
             :IsDeleted, :DeleterUserId, :DeletionTime, :TenantId)
            RETURNING ID INTO :Id";

        using var connection = CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("FileName", document.FileName);
        parameters.Add("Extension", document.Extension);
        parameters.Add("FilePath", document.Path);
        parameters.Add("FileSize", document.Size);
        parameters.Add("TypeId", document.TypeId);
        parameters.Add("CurrentVersion", document.CurrentVersion);
        parameters.Add("CreationTime", document.CreationTime);
        parameters.Add("CreatorUserId", document.CreatorUserId);
        parameters.Add("LastModificationTime", document.LastModificationTime);
        parameters.Add("LastModifierUserId", document.LastModifierUserId);
        parameters.Add("IsDeleted", document.IsDeleted);
        parameters.Add("DeleterUserId", document.DeleterUserId);
        parameters.Add("DeletionTime", document.DeletionTime);
        parameters.Add("TenantId", document.TenantId);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        var id = parameters.Get<int>("Id");
        _logger.LogDebug("Doküman eklendi, ID: {Id}", id);

        return id;
    }

    public async Task InsertDocumentIndexesAsync(IEnumerable<DmsDocumentIndex> indexes)
    {
        if (!indexes.Any()) return;

        const string sql = @"
            INSERT INTO DMS_DOCUMENT_INDEX 
            (VALUE, DOCUMENT_ID, INDEX_ID, CREATION_TIME, CREATOR_USER_ID,
             LAST_MODIFICATION_TIME, LAST_MODIFIER_USER_ID, IS_DELETED, 
             DELETER_USER_ID, DELETION_TIME, TENANT_ID)
            VALUES 
            (:Value, :DocumentId, :IndexId, :CreationTime, :CreatorUserId,
             :LastModificationTime, :LastModifierUserId, :IsDeleted,
             :DeleterUserId, :DeletionTime, :TenantId)";

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, indexes);

        _logger.LogDebug("{Count} index eklendi", indexes.Count());
    }

    public async Task<bool> DocumentExistsAsync(string fileName)
    {
        const string sql = "SELECT COUNT(1) FROM DMS_DOCUMENT WHERE FILE_NAME = :FileName AND IS_DELETED = 0";

        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { FileName = fileName });

        return count > 0;
    }

    public async Task<DmsIndex?> GetIndexByKeyAsync(string key)
    {
        const string sql = @"
            SELECT ID as Id, KEY as Key, CREATION_TIME as CreationTime, 
                   CREATOR_USER_ID as CreatorUserId, LAST_MODIFICATION_TIME as LastModificationTime,
                   LAST_MODIFIER_USER_ID as LastModifierUserId, IS_DELETED as IsDeleted,
                   DELETER_USER_ID as DeleterUserId, DELETION_TIME as DeletionTime,
                   TENANT_ID as TenantId
            FROM DMS_INDEX 
            WHERE KEY = :Key AND IS_DELETED = 0";

        using var connection = CreateConnection();
        var index = await connection.QueryFirstOrDefaultAsync<DmsIndex>(sql, new { Key = key });

        return index;
    }

    public async Task<int> InsertIndexAsync(DmsIndex index)
    {
        const string sql = @"
            INSERT INTO DMS_INDEX 
            (KEY, CREATION_TIME, CREATOR_USER_ID, LAST_MODIFICATION_TIME, 
             LAST_MODIFIER_USER_ID, IS_DELETED, DELETER_USER_ID, DELETION_TIME, TENANT_ID)
            VALUES 
            (:Key, :CreationTime, :CreatorUserId, :LastModificationTime,
             :LastModifierUserId, :IsDeleted, :DeleterUserId, :DeletionTime, :TenantId)
            RETURNING ID INTO :Id";

        using var connection = CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("Key", index.Key);
        parameters.Add("CreationTime", index.CreationTime);
        parameters.Add("CreatorUserId", index.CreatorUserId);
        parameters.Add("LastModificationTime", index.LastModificationTime);
        parameters.Add("LastModifierUserId", index.LastModifierUserId);
        parameters.Add("IsDeleted", index.IsDeleted);
        parameters.Add("DeleterUserId", index.DeleterUserId);
        parameters.Add("DeletionTime", index.DeletionTime);
        parameters.Add("TenantId", index.TenantId);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);

        var id = parameters.Get<int>("Id");
        _logger.LogDebug("Index tanımı eklendi, ID: {Id}", id);

        return id;
    }

    public async Task BulkInsertDocumentsAsync(IEnumerable<DmsDocument> documents)
    {
        if (!documents.Any()) return;

        const string sql = @"
            INSERT INTO DMS_DOCUMENT 
            (FILE_NAME, EXTENSION, PATH, SIZE, TYPE_ID, CURRENT_VERSION, CREATION_TIME, 
             CREATOR_USER_ID, LAST_MODIFICATION_TIME, LAST_MODIFIER_USER_ID, 
             IS_DELETED, DELETER_USER_ID, DELETION_TIME, TENANT_ID)
            VALUES 
            (:FileName, :Extension, :Path, :Size, :TypeId, :CurrentVersion, :CreationTime,
             :CreatorUserId, :LastModificationTime, :LastModifierUserId,
             :IsDeleted, :DeleterUserId, :DeletionTime, :TenantId)";

        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var parameters = documents.Select(doc => new
            {
                doc.FileName,
                doc.Extension,
                doc.Path,
                doc.Size,
                doc.TypeId,
                doc.CurrentVersion,
                doc.CreationTime,
                doc.CreatorUserId,
                doc.LastModificationTime,
                doc.LastModifierUserId,
                doc.IsDeleted,
                doc.DeleterUserId,
                doc.DeletionTime,
                doc.TenantId
            });

            await connection.ExecuteAsync(sql, parameters, transaction);
            transaction.Commit();

            _logger.LogInformation("{Count} doküman toplu olarak eklendi", documents.Count());
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Toplu doküman ekleme sırasında hata");
            throw;
        }
    }

    public async Task BulkInsertIndexesAsync(IEnumerable<DmsDocumentIndex> indexes)
    {
        if (!indexes.Any()) return;

        const string sql = @"
            INSERT INTO DMS_DOCUMENT_INDEX 
            (VALUE, DOCUMENT_ID, INDEX_ID, CREATION_TIME, CREATOR_USER_ID,
             LAST_MODIFICATION_TIME, LAST_MODIFIER_USER_ID, IS_DELETED, 
             DELETER_USER_ID, DELETION_TIME, TENANT_ID)
            VALUES 
            (:Value, :DocumentId, :IndexId, :CreationTime, :CreatorUserId,
             :LastModificationTime, :LastModifierUserId, :IsDeleted,
             :DeleterUserId, :DeletionTime, :TenantId)";

        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(sql, indexes, transaction);
            transaction.Commit();

            _logger.LogInformation("{Count} index toplu olarak eklendi", indexes.Count());
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Toplu index ekleme sırasında hata");
            throw;
        }
    }
}
