using AutoMapper;
using ChargingApp.Interfaces;

namespace ChargingApp.Data;

public class UnitOfWork
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private CategoryRepository? _categoryRepo;
    private OrdersRepository? _orderRepo;
    private PaymentGatewayRepository? _paymentGatewayRepo;
    private PaymentRepository? _paymentRepo;
    private ProductRepository? _productRepo;


    public UnitOfWork(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public CategoryRepository CategoryRepo
    {
        get
        {
            if (_categoryRepo is null) _categoryRepo = new CategoryRepository(_context, _mapper);
            return _categoryRepo;
        }
    }

    public OrdersRepository OrdersRepo
    {
        get
        {
            if (_orderRepo is null) 
                _orderRepo = new OrdersRepository(_context, _mapper);
            return _orderRepo;
        }
    }

    public PaymentGatewayRepository PaymentGatewayRepo
    {
        get
        {
            if (_paymentGatewayRepo is null) 
                _paymentGatewayRepo = new PaymentGatewayRepository(_context);
            return _paymentGatewayRepo;
        }
    }


    public PaymentRepository PaymentRepo
    {
        get
        {
            if (_paymentRepo is null)
                _paymentRepo = new PaymentRepository(_context, _mapper);
            return _paymentRepo;
        }
    }
    
    public ProductRepository ProductRepo
    {
        get
        {
            if (_productRepo is null) 
                _productRepo = new ProductRepository(_context);
            return _productRepo;
        }
    }
}