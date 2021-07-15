using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Entities;
using ClickersAPI.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/account_data_fields")]
    public class AccountDataFieldsController : ControllerBaseImpl
    {
        public const int AllAccountIdsKey = -1;
        
        public AccountDataFieldsController
            (ApplicationDbContext _Context,
            IMapper _Mapper, 
            IServiceProvider _Provider) 
            : base(_Context, _Mapper, _Provider) { }
        
        [HttpPost("get_list")]
        public async Task<ActionResult<List<AccountFieldDto>>> GetList(
            [FromBody] AccountFieldListDtoLite _RequestDtos)
        { 
            var dataFields = _RequestDtos.DataFields;
            var pagination = _RequestDtos.Pagination;
            var queryable = Context
                .Set<AccountDataField>()
                .AsNoTracking()
                .ToList();
            var fullDtosList = dataFields.ToList();
            
            foreach (var requestDto in fullDtosList.ToList())
            {
                if (requestDto.AccountId != AllAccountIdsKey)
                    continue;
                var ac = new AccountsController(Context, Mapper, Provider);
                var accounts = await ac.GetAccounts();
                fullDtosList.Remove(requestDto);
                fullDtosList.AddRange(accounts
                    .Select(_AccountDto => new AccountFieldDtoLite
                    {
                        AccountId = _AccountDto.Id,
                        FieldId = requestDto.FieldId
                    }));
            }

            var groups = 
                fullDtosList.GroupBy(_DtoLite => _DtoLite.AccountId);
            var filteredQueryable = queryable.Where(_Item =>
                groups.Where(_Group => _Group.Key == _Item.AccountId)
                    .SelectMany(_G => _G)
                    .Any(_DtoItem => _Item.FieldId == _DtoItem.FieldId)).AsQueryable();
            
            if (pagination != null)
                await HttpContext.InsertPaginationParametersResponse(
                    filteredQueryable, pagination.RecordsPerPage);

            List<AccountDataField> entities = pagination == null
                ? await filteredQueryable.ToListAsyncSafe()
                : await filteredQueryable.Paginate(pagination).ToListAsyncSafe();

            return Mapper.Map<List<AccountFieldDto>>(entities);
        }

        [HttpPost("set_list")]
        public async Task<ActionResult> SetList(
            [FromBody] List<AccountFieldDto> _RequestDtos)
        {
            var dataFields = _RequestDtos
                .Select(_Df => _Df as AccountFieldDtoLite)
                .ToList();
            var dfList = new AccountFieldListDtoLite{ DataFields = dataFields };
            var dfsToPatch = (await GetList(dfList)).Value;
            dfsToPatch.ForEach(_Dto => _Dto.Value = _RequestDtos.First(_Rdto => 
                _Rdto.AccountId == _Dto.AccountId && _Rdto.FieldId == _Dto.FieldId).Value);
            var dfsToPost = _RequestDtos
                .Where(_Df => dfsToPatch.All(_DfToPatch =>
                    _Df.AccountId != _DfToPatch.AccountId
                    && _Df.FieldId != _DfToPatch.FieldId))
                .ToList();
            await PatchList(dfsToPatch);
            await PostList(dfsToPost);
            return Ok();
        }
        
        public async Task<ActionResult> PatchAccountDataField(
            [FromQuery] AccountFieldDto _Dto,
            [FromQuery] JsonPatchDocument<AccountFieldDto> _PatchDocument)
        {
            if (_PatchDocument == null)
                return BadRequest(ResponseErrors.RequestIsIncorrect);
            var entities = await GetFields<AccountDataField, AccountFieldDto>(_Dto);
            if (!entities.Any())
                return NotFound(ResponseErrors.EntityDoesNotExist);

            var entity = entities.First();
            var entityDto = Mapper.Map<AccountFieldDto>(entity);
            _PatchDocument.ApplyTo(entityDto, ModelState);
            
            if (!TryValidateModel(entityDto))
                return BadRequest(ResponseErrors.DbValidationFail(ModelState));
            
            var updatedEntity = Mapper.Map(entityDto, entity);
            Context.Update(updatedEntity);
            await Context.SaveChangesAsync();
            return Accepted();
        }
        
        private async Task PatchList(IEnumerable<AccountFieldDto> _RequestDtos)
        {
            foreach (var dto in _RequestDtos)
            {
                var patchDoc = new JsonPatchDocument<AccountFieldDto>();
                patchDoc.Replace(_E => _E.Value, dto.Value);
                patchDoc.Replace(_E => _E.LastUpdate, DateTime.Now);
                await PatchAccountDataField(dto, patchDoc);
            }
        }
        
        private async Task PostList(IEnumerable<AccountFieldDto> _RequestDtos)
        {
            foreach (var dto in _RequestDtos)
                await Post<AccountFieldDto, AccountDataField, AccountFieldDto>(dto);
        }
    }
}