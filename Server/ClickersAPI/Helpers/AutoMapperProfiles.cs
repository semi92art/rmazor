using System;
using ClickersAPI.DTO;
using ClickersAPI.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClickersAPI.Helpers
{
    public class AutoMapperProfiles : AutoMapper.Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Account, AccountDto>().ReverseMap();
            CreateMap<AccountCreationDto, Account>();
            CreateMap<AccountCreationDto, AccountFindDto>();
            CreateMap<AccountFindDto, AccountDto>();
            CreateMap<Account, FindDto>().ReverseMap();

            CreateMap<AccountFieldDtoLite, AccountDataField>();
            CreateMap<AccountDataField, AccountFieldDto>().AfterMap(AccountDataFieldDtoAfterMap);
            CreateMap<AccountFieldDto, AccountDataField>().AfterMap(AccountDataFieldAfterMap);
            
            CreateMap<GameFieldDtoLite, GameDataField>();
            CreateMap<GameDataField, GameFieldDto>().AfterMap(GameDataFieldDtoAfterMap);
            CreateMap<GameFieldDto, GameDataField>().AfterMap(GameDataFieldAfterMap);

            CreateMap<IdentityUser, UserDto>()
                .ForMember(_X => _X.EmailAddress, 
                    _Options => 
                        _Options.MapFrom(_X => _X.Email))
                .ForMember(_X => _X.UserId, 
                    _Options =>
                        _Options.MapFrom(_X => _X.Id));
        }

        private void AccountDataFieldDtoAfterMap(AccountDataField _Entity, AccountFieldDto _Dto)
        {
            DataFieldDtoAfterMap(_Entity, _Dto);
        }
        
        private void GameDataFieldDtoAfterMap(GameDataField _Entity, GameFieldDto _Dto)
        {
            DataFieldDtoAfterMap(_Entity, _Dto);
        }

        private void AccountDataFieldAfterMap(AccountFieldDto _Dto, AccountDataField _Entity)
        {
            DataFieldAfterMap(_Dto, _Entity);
        }
        
        private void GameDataFieldAfterMap(GameFieldDto _Dto, GameDataField _Entity)
        {
            DataFieldAfterMap(_Dto, _Entity);
        }

        private void DataFieldDtoAfterMap<TEntity, TDto>(TEntity _Entity, TDto _Dto)
            where TDto : IDataFieldValueDto
            where TEntity : IDataFieldValue
        {
            if (_Entity.NumericValue.HasValue)
                _Dto.Value = _Entity.NumericValue.Value;
            else if (!string.IsNullOrEmpty(_Entity.StringValue))
                _Dto.Value = _Entity.StringValue;
            else if (_Entity.BoolValue.HasValue)
                _Dto.Value = _Entity.BoolValue.Value;
            else if (_Entity.FloatingValue.HasValue)
                _Dto.Value = _Entity.FloatingValue.Value;
            else if (_Entity.DateTimeValue.HasValue)
                _Dto.Value = _Entity.DateTimeValue;
        }

        private void DataFieldAfterMap<TDto, TEntity>(TDto _Dto, TEntity _Entity)
            where TDto : IDataFieldValueDto
            where TEntity : IDataFieldValue
        {
            switch (_Dto.Value)
            {
                case short _: case int _: case long _:
                    _Entity.NumericValue = Convert.ToInt64(_Dto.Value);
                    ClearDataFieldValues(_Entity, nameof(_Entity.NumericValue));
                    break;
                case string value:
                    _Entity.StringValue = value; 
                    ClearDataFieldValues(_Entity, nameof(_Entity.StringValue));
                    break;
                case bool value:
                    _Entity.BoolValue = value; 
                    ClearDataFieldValues(_Entity, nameof(_Entity.BoolValue));
                    break;
                case float _: case double _: case decimal _:
                    _Entity.FloatingValue = Convert.ToDecimal(_Dto.Value); 
                    ClearDataFieldValues(_Entity, nameof(_Entity.FloatingValue));
                    break;
                case DateTime value:
                    _Entity.DateTimeValue = value;
                    ClearDataFieldValues(_Entity, nameof(_Entity.DateTimeValue));
                    break;
            }
        }

        private void ClearDataFieldValues<TEntity>(TEntity _Dfv, string _NameOfNewValue)
            where TEntity : IDataFieldValue
        {
            if (nameof(_Dfv.NumericValue) != _NameOfNewValue)
                _Dfv.NumericValue = default;
            if (nameof(_Dfv.StringValue) != _NameOfNewValue)
                _Dfv.StringValue = default;
            if (nameof(_Dfv.BoolValue) != _NameOfNewValue)
                _Dfv.BoolValue = default;
            if (nameof(_Dfv.FloatingValue) != _NameOfNewValue)
                _Dfv.FloatingValue = default;
            if (nameof(_Dfv.DateTimeValue) != _NameOfNewValue)
                _Dfv.DateTimeValue = default;
        }
    }
}
