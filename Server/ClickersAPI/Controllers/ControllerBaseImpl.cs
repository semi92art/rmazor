using System;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Entities;
using ClickersAPI.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ClickersAPI.Controllers
{
    public abstract class ControllerBaseImpl : ControllerBase
    {
        #region protected members
        
        protected ApplicationDbContext Context { get; }
        protected IMapper Mapper { get; }
        protected IServiceProvider Provider { get; }
        
        #endregion

        #region constructor

        protected ControllerBaseImpl(
            ApplicationDbContext _Context, IMapper _Mapper, IServiceProvider _Provider)
        {
            Context = _Context;
            Mapper = _Mapper;
            Provider = _Provider;
        }

        #endregion
        
        #region GET methods
        
        protected async Task<List<TDto>> Get<TEntity, TDto>(PaginationDto _Pagination) 
            where TEntity : class
        {
            var queryable = Context.Set<TEntity>().AsNoTracking().AsQueryable();
            await HttpContext.InsertPaginationParametersResponse(queryable, _Pagination.RecordsPerPage);
            List<TEntity> entities = await queryable.Paginate(_Pagination).ToListAsync();
            return Mapper.Map<List<TDto>>(entities);
        }
        
        protected async Task<ActionResult<TDto>> Get<TEntity, TDto>(int _Id) 
            where TEntity : class, IId
        {
            var queryable = Context.Set<TEntity>().AsQueryable();
            return await GetDto<TEntity, TDto>(_Id, queryable);
        }

        protected async Task<ActionResult<TDto>> GetDto<TEntity, TDto>(int _Id, IQueryable<TEntity> _Queryable) 
            where TEntity : class, IId
        {
            TEntity entity = await _Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(_X => _X.Id == _Id);
            if (entity != null) 
                return Mapper.Map<TDto>(entity);
            var response = new ErrorResponse(202, 
                $"Entity {nameof(TEntity)} with this AccountId and GameId does not exist");
            return BadRequest(response);
        }

        protected async Task<List<TEntity>> GetFields<TEntity, TDto>(TDto _Dto)
            where TEntity : class, IAccountId, IFieldId
            where TDto : IAccountId, IFieldId
        {
            var queryable = Context.Set<TEntity>().AsNoTracking();
            var entities = await queryable.ToListAsyncSafe();
            var result = entities.Where(_E =>
                _E.AccountId == _Dto.AccountId
                && _E.FieldId == _Dto.FieldId);
            return result.ToList();
        }

        protected async Task<List<TDto>> GetDtoList<TEntity, TDtoLite, TDto>(
            List<TDtoLite> _DtosLite, PaginationDto _Pagination)
            where TEntity : class, IAccountId, IGameId
            where TDtoLite : IAccountId, IGameId
        {
            var queryable = Context.Set<TEntity>().AsNoTracking();
            var filtered = queryable.Where(_Entity =>
                _DtosLite.Any(_Dto => _Dto.AccountId == _Entity.AccountId 
                                      && _Dto.GameId == _Entity.GameId))
                .AsQueryable();
            await HttpContext.InsertPaginationParametersResponse(filtered, _Pagination.RecordsPerPage);
            List<TEntity> entities = await filtered.Paginate(_Pagination).ToListAsync();
            return Mapper.Map<List<TDto>>(entities);
        }
        
        #endregion

        #region POST methods
        
        protected async Task<ActionResult<TRead>> Post<TCreation, TEntity, TRead>(TCreation _Creation) 
            where TEntity : class, IId
        {
            TEntity entity = Mapper.Map<TEntity>(_Creation);
            if (entity is ICreationTime creationTimeEntity)
                creationTimeEntity.CreationTime = DateTime.Now;
            Context.Add(entity);
            await Context.SaveChangesAsync();
            return await Get<TEntity, TRead>(entity.Id);
        }
        
        #endregion
        
        #region DELETE methods
        
        protected async Task<ActionResult> Delete<TEntity>(int _Id) 
            where TEntity : class, IId, new()
        {
            using (IServiceScope scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ApplicationDbContext context = Provider.GetService<ApplicationDbContext>();
                bool exists = await context.Set<TEntity>().AnyAsync(_X => _X.Id == _Id);
                if (!exists)
                    return NotFound(ResponseErrors.EntityDoesNotExist);

                context.Remove(new TEntity {Id = _Id});
                await context.SaveChangesAsync();
            }
            return Accepted();
        }
        
        #endregion

        protected async Task<ActionResult> UpdateEntity<TEntity, TDto>(TEntity _Entity, TDto _Dto)
            where TEntity : class
            where TDto : class
        {
            if (!TryValidateModel(_Dto))
                return BadRequest(ResponseErrors.DbValidationFail(ModelState));
            var updatedEntity = Mapper.Map(_Dto, _Entity);
            Context.Update(updatedEntity);
            await Context.SaveChangesAsync();
            return Accepted();
        }
    }
}
