using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class CmsRepository : ICmsRepository
{
    private readonly IUnitOfWork _uow;

    public CmsRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<CmsPage>> GetAllAsync()
    {
        const string sql = "SELECT id, title, slug, content, created_at as CreatedAt, updated_at as UpdatedAt FROM cms_pages WHERE deleted_at IS NULL ORDER BY id";
        return await _uow.Connection.QueryAsync<CmsPage>(sql, null, _uow.Transaction);
    }

    public async Task<CmsPage?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, title, slug, content, created_at as CreatedAt, updated_at as UpdatedAt FROM cms_pages WHERE id = @id AND deleted_at IS NULL";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CmsPage>(sql, new { id }, _uow.Transaction);
    }

    public async Task<CmsPage?> GetBySlugAsync(string slug)
    {
        const string sql = "SELECT id, title, slug, content, created_at as CreatedAt, updated_at as UpdatedAt FROM cms_pages WHERE slug = @slug AND deleted_at IS NULL";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CmsPage>(sql, new { slug }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(CmsPage page)
    {
        const string sql = @"
            INSERT INTO cms_pages (title, slug, content, created_at, created_by)
            VALUES (@Title, @Slug, @Content, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";

        return await _uow.Connection.ExecuteScalarAsync<long>(sql, new
        {
            page.Title,
            page.Slug,
            page.Content,
            CreatedBy = page.CreatedBy
        }, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(CmsPage page)
    {
        const string sql = @"
            UPDATE cms_pages 
            SET title = @Title, slug = @Slug, content = @Content, updated_at = CURRENT_TIMESTAMP, updated_by = @UpdatedBy
            WHERE id = @Id AND deleted_at IS NULL";

        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new
        {
            page.Id,
            page.Title,
            page.Slug,
            page.Content,
            UpdatedBy = page.UpdatedBy
        }, _uow.Transaction);

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id, long? deletedBy)
    {
        const string sql = @"
            UPDATE cms_pages 
            SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @DeletedBy 
            WHERE id = @id AND deleted_at IS NULL";

        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { id, DeletedBy = deletedBy }, _uow.Transaction);
        return rowsAffected > 0;
    }
}
