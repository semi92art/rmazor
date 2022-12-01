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
    [Route("api/game_data_fields")]
    public class GameDataFieldsController : ControllerBaseImpl
    {
        public const ushort MaxFieldId = ushort.MaxValue;
        private const int AllGameIdsKey = -1;

        public GameDataFieldsController(
            ApplicationDbContext _Context,
            IMapper              _Mapper,
            IServiceProvider     _Provider)
            : base(_Context, _Mapper, _Provider) { }

        [HttpPost("get_list")]
        public async Task<ActionResult<List<GameFieldDto>>> GetList(
            [FromBody] GameFieldListDtoLite _RequestDtos)
        {
            var dataFields = _RequestDtos.DataFields;
            var pagination = _RequestDtos.Pagination;
            var queryable = Context
                .Set<GameDataField>()
                .AsNoTracking()
                .ToList();
            var fullDtosList = dataFields.ToList();
            
            foreach (var requestDto in fullDtosList.ToList())
            {
                if (requestDto.AccountId != AccountDataFieldsController.AllAccountIdsKey)
                    continue;
                var ac = new AccountsController(Context, Mapper, Provider);
                var accounts = await ac.GetAccounts();
                fullDtosList.Remove(requestDto);
                fullDtosList.AddRange(accounts
                    .Select(_AccountDto => new GameFieldDtoLite
                    {
                        AccountId = _AccountDto.Id,
                        GameId = requestDto.GameId, 
                        FieldId = requestDto.FieldId
                    }));
            }
            
            foreach (var requestDto in fullDtosList.ToList())
            {
                if (requestDto.GameId != AllGameIdsKey)
                    continue;
                fullDtosList.Remove(requestDto);
                IEnumerable<int> gameIds = Enumerable.Range(1, 10);
                fullDtosList.AddRange(gameIds
                    .Select(_GameId => new GameFieldDtoLite
                    {
                        AccountId = requestDto.AccountId,
                        GameId = _GameId,
                        FieldId = requestDto.FieldId
                    }));
            }

            var groups = 
                fullDtosList.GroupBy(_DtoLite => new {_DtoLite.AccountId, _DtoLite.GameId});
            var filteredQueryable = queryable.Where(_Item =>
                groups.Where(_Group =>
                        _Group.Key.AccountId == _Item.AccountId && _Group.Key.GameId == _Item.GameId)
                    .SelectMany(_G => _G)
                    .Any(_DtoItem => _Item.FieldId == _DtoItem.FieldId)).AsQueryable();
            
            if (pagination != null)
                await HttpContext.InsertPaginationParametersResponse(
                    filteredQueryable, pagination.RecordsPerPage);

            List<GameDataField> entities = pagination == null
                ? await filteredQueryable.ToListAsyncSafe()
                : await filteredQueryable.Paginate(pagination).ToListAsyncSafe();

            return Mapper.Map<List<GameFieldDto>>(entities);
        }

        [HttpPost("set_list")]
        public async Task<ActionResult> SetList(
            [FromBody] List<GameFieldDto> _RequestDtos)
        {
            var dataFields = _RequestDtos
                .Select(_Df => _Df as GameFieldDtoLite)
                .ToList();
            var dfList = new GameFieldListDtoLite{ DataFields = dataFields };
            var dfsToPatch = (await GetList(dfList)).Value;
            dfsToPatch.ForEach(_Dto => _Dto.Value = _RequestDtos.First(_Rdto => 
                _Rdto.AccountId == _Dto.AccountId
                && _Rdto.GameId == _Dto.GameId
                && _Rdto.FieldId == _Dto.FieldId).Value);
            var dfsToPost = _RequestDtos
                .Where(_Df => dfsToPatch.All(_DfToPatch =>
                    _Df.AccountId != _DfToPatch.AccountId
                    && _Df.GameId != _DfToPatch.GameId
                    && _Df.FieldId != _DfToPatch.FieldId))
                .ToList();
            await PatchList(dfsToPatch);
            await PostList(dfsToPost);
            return Ok();
        }
        
        public async Task<ActionResult> PatchGameDataField(
            [FromQuery] GameFieldDto _Dto,
            [FromQuery] JsonPatchDocument<GameFieldDto> _PatchDocument)
        {
            if (_PatchDocument == null)
                return BadRequest(ResponseErrors.RequestIsIncorrect);
            var entities = await GetFields<GameDataField, GameFieldDto>(_Dto);
            if (!entities.Any())
                return NotFound(ResponseErrors.EntityDoesNotExist);

            var entity = entities.FirstOrDefault(_E =>
                    _E is IGameId gameId && gameId.GameId == _Dto.GameId);
            var entityDto = Mapper.Map<GameFieldDto>(entity);
            _PatchDocument.ApplyTo(entityDto, ModelState);

            return await UpdateEntity(entity, entityDto);
        }

        private async Task PatchList(IEnumerable<GameFieldDto> _RequestDtos)
        {
            foreach (var dto in _RequestDtos)
            {
                var patchDoc = new JsonPatchDocument<GameFieldDto>();
                patchDoc.Replace(_E => _E.Value, dto.Value);
                patchDoc.Replace(_E => _E.LastUpdate, DateTime.Now);
                await PatchGameDataField(dto, patchDoc);
            }
        }
        
        private async Task PostList(IEnumerable<GameFieldDto> _RequestDtos)
        {
            foreach (var dto in _RequestDtos)
                await Post<GameFieldDto, GameDataField, GameFieldDto>(dto);
        }
    }
}