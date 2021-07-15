using System;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Entities;
using ClickersAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBaseImpl
    {
        private const string TestUserPrefix = "test";
        
        public AccountsController(ApplicationDbContext _Context, IMapper _Mapper, IServiceProvider _Provider) 
            : base(_Context, _Mapper, _Provider) { }
        
        #region api

        public async Task<List<AccountDto>> GetAccounts()
        {
            var accounts = await Context.Set<Account>().ToListAsyncSafe();
            return Mapper.Map<List<AccountDto>>(accounts);
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<AccountDto>> Login([FromBody] AccountFindDto _AccDto)
        {
            Account acc = await Context.Accounts
                .FirstOrDefaultAsync(_Acc =>
                    _Acc.Name == _AccDto.Name
                    && _Acc.PasswordHash == _AccDto.PasswordHash);
            if (acc != null)
                return Mapper.Map<AccountDto>(acc);
            
            acc = await Context.Accounts.FirstOrDefaultAsync(_Acc => _Acc.Name == _AccDto.Name);
            return NotFound(acc == null ? ResponseErrors.AccountDoesNotExist(_AccDto.Name) :
                ResponseErrors.LoginOrPasswordIncorrect);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AccountDto>> Register([FromBody] AccountCreationDto _AccountCreation)
        {
            if (await Context.Accounts.AnyAsync(_Acc => _Acc.Name == _AccountCreation.Name))
                return Conflict(ResponseErrors.AccountWithNameAlreadyExist);
            return await Post<AccountCreationDto, Account, AccountDto>(_AccountCreation);
        }

        [HttpGet("delete_test_accounts")]
        public async Task<ActionResult> DeleteTestAccounts()
        {
            var accounts = await Context.Accounts
                .AsNoTracking()
                .Where(_Acc => _Acc.Name.StartsWith(TestUserPrefix) 
                              && _Acc.Name != "test_user_do_not_delete")
                .ToArrayAsync();
            await DeleteAccounts(accounts);
            return Ok("deleted");
        }

        #endregion

        #region private methods
        
        private async Task DeleteAccounts(IEnumerable<Account> _Accounts)
        {
            var accounts = _Accounts as Account[] ?? _Accounts.ToArray();
            var accIds = accounts.Select(_Acc => _Acc.Id).ToArray();
            await DeleteAccountFields(accIds);
            await DeleteGameFields(accIds);
            foreach (var account in accounts)
                await Delete<Account>(account.Id);
        }

        private async Task DeleteAccountFields(IEnumerable<int> _AccountIds)
        {
            var afc = new AccountDataFieldsController(Context, Mapper, Provider);
            var dataFields = _AccountIds
                .ToList()
                .SelectMany(_AccId => 
                    Enumerable.Range(0, GameDataFieldsController.MaxFieldId).Select(_FieldId =>
                            new AccountFieldDtoLite {
                                AccountId = _AccId, FieldId = Convert.ToUInt16(_FieldId)
                            }))
                .ToList();
            var dfList = new AccountFieldListDtoLite{DataFields = dataFields};
            var existingDataFieldDtos = (await afc.GetList(dfList)).Value;
            foreach (var existingDataFieldDto in existingDataFieldDtos.ToList())
            {
                var existingDataField =
                    (await GetFields<AccountDataField, AccountFieldDto>(existingDataFieldDto)).First();
                if (existingDataField != null)
                    await Delete<AccountDataField>(existingDataField.Id);
            }
        }

        private async Task DeleteGameFields(IEnumerable<int> _AccountIds)
        {
            var dfc = new GameDataFieldsController(Context, Mapper, Provider);
            var dataFields = _AccountIds
                .ToList()
                .SelectMany(_AccId => Enumerable.Range(0, 10)
                    .SelectMany(_GameId => Enumerable.Range(0, GameDataFieldsController.MaxFieldId)
                        .Select(_FieldId =>
                            new GameFieldDtoLite {
                                AccountId = _AccId, GameId = _GameId, FieldId = Convert.ToUInt16(_FieldId)
                            })))
                .ToList();
            var dfList = new GameFieldListDtoLite{DataFields = dataFields};
            var existingDataFieldDtos = (await dfc.GetList(dfList)).Value;
            foreach (var existingDataFieldDto in existingDataFieldDtos.ToList())
            {
                var existing = await GetFields<GameDataField, GameFieldDto>(existingDataFieldDto);
                var existingDataField = existing
                    .FirstOrDefault(_E => _E is IGameId gameId
                                          && gameId.GameId == existingDataFieldDto.GameId);
                if (existingDataField != null)
                    await Delete<GameDataField>(existingDataField.Id);
            }
        }

        #endregion
    }
}
