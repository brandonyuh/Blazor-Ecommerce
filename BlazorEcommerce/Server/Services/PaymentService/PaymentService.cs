using Stripe;
using Stripe.Checkout;

namespace BlazorEcommerce.Server.Services.PaymentService
{
	public class PaymentService : IPaymentService
	{
		private readonly ICartService _cartService;
		private readonly IAuthService _authService;
		private readonly IOrderService _orderService;

		const string secret = "whsec_cb51a6c551514a238f06fd31a7584269d8f55d29713866a83316ab23ebf4040b";

		public PaymentService(ICartService cartService, IAuthService authService, IOrderService orderService)
		{
			StripeConfiguration.ApiKey = "sk_test_51NKOPzBo2k2XDi6NJFJ8epLK83TdR3N6CIjzmC3IDOiMfgtk8dyAJ6EVOlKWZrvDZ9GNC9qYFeDKjqXFR0yaFfwg00of42Yof8";


			_cartService = cartService;
			_authService = authService;
			_orderService = orderService;
		}
		public async Task<Session> CreateCheckoutSession()
		{
			var products = (await _cartService.GetDbCartProducts()).Data;
			var lineItem = new List<SessionLineItemOptions>();
			products.ForEach(product => lineItem.Add(new SessionLineItemOptions
			{
				PriceData = new SessionLineItemPriceDataOptions
				{
					UnitAmountDecimal = product.Price * 100,
					Currency = "usd",
					ProductData = new SessionLineItemPriceDataProductDataOptions
					{
						Name = product.Title,
						Images = new List<string> { product.ImageUrl }
					}
				},
				Quantity = product.Quantity
			}));

			var options = new SessionCreateOptions
			{
				CustomerEmail = _authService.GetUserEmail(),
				ShippingAddressCollection = new SessionShippingAddressCollectionOptions
				{
					AllowedCountries = new List<string>
					{
						"US",
						"CA",
					}
				},
				PaymentMethodTypes = new List<string>
				{
					"card",
				},
				LineItems = lineItem,
				Mode = "payment",
				SuccessUrl = "https://localhost:7093/order-success",
				CancelUrl = "https://localhost:7093/cart"
			};
			var service = new SessionService();
			Session session = service.Create(options);
			return session;
		}

		public async Task<ServiceResponse<bool>> FulfillOrder(HttpRequest request)
		{
			var json = await new StreamReader(request.Body).ReadToEndAsync();
			try
			{
				var stripeEvent = EventUtility.ConstructEvent(json, request.Headers["Stripe-Signature"], secret);
				if (stripeEvent.Type == Events.CheckoutSessionCompleted)
				{
					var session = stripeEvent.Data.Object as Session;
					var user = await _authService.GetUserByEmail(session.CustomerEmail);
					await _orderService.PlaceOrder(user.Id);
				}
				return new ServiceResponse<bool>
				{
					Data = true,
				};
			}
			catch (StripeException e)
			{
				return new ServiceResponse<bool>
				{
					Data = false,
					Success = false,
					Message = e.Message
				};
			}
		}
	}
}
