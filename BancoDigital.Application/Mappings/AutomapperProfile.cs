using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BancoDigital.Application.DTOs;
using BancoDigital.Application.Requests;
using BancoDigital.Application.Responses;
using BancoDigital.Domain.Entities;

namespace BancoDigital.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Cliente
            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<Cliente, ClienteResponse>();
            CreateMap<ClienteRequest, Cliente>();

            // Solicitação Crédito
            CreateMap<SolicitacaoCredito, SolicitacaoCreditoDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<SolicitacaoCredito, SolicitacaoCreditoResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<SolicitacaoCreditoRequest, SolicitacaoCredito>();
        }
    }
}