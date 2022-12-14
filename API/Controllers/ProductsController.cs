using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    
    public class ProductsController : BaseApiController
    {
        public IGenericRepository<Product> _productsRepo { get; }
        public IGenericRepository<ProductBrand> _producBrandRepo { get; }
        public IGenericRepository<ProductType> _productTypeRepo { get; }
        public IMapper _mapper {get;}
        
        public ProductsController(IGenericRepository<Product> productsRepo, 
        IGenericRepository<ProductBrand> producBrandRepo, 
        IGenericRepository<ProductType> productTypeRepo, 
        IMapper mapper)
        {
            _productsRepo = productsRepo;
            _productTypeRepo = productTypeRepo;            
            _producBrandRepo = producBrandRepo;           
            _mapper = mapper;
        }

        [HttpGet]
        public async Task< ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
            [FromQuery]ProductSpecParams producParams)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(producParams);
            var countSpec = new ProductsWithFiltersForCountSpecification(producParams);
            var totalItems = await _productsRepo.CountAsync(countSpec);

            var products = await _productsRepo.ListAsync(spec);

            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            return Ok(new Pagination<ProductToReturnDto>(producParams.PageIndex, producParams.PageSize, totalItems, data));
        }




        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product =  await _productsRepo.GetEntityWithSpec(spec);
            
            if(product == null)
            {
                return NotFound(new ApiResponse(404));
            }

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<ProductBrand>> GetProductBrands()
        {
            return Ok( await _producBrandRepo.ListAllAsync());
        }

        [HttpGet("types")]
        public async Task<ActionResult<ProductBrand>> GetProductTypes()
        {
            return Ok( await _productTypeRepo.ListAllAsync());
        }


        
    }
}