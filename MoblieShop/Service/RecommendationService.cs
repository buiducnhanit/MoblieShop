using Microsoft.ML;
using MoblieShop.Models;
using MoblieShop.Repository;

namespace MoblieShop.Service
{
    public class RecommendationService
    {
        private readonly IRecommendationRepository _recommendationRepository;
        private readonly MLContext _mlContext;

        public RecommendationService(IRecommendationRepository recommendationRepository)
        {
            _recommendationRepository = recommendationRepository;
            _mlContext = new MLContext();
        }

        // Phương thức để lấy gợi ý sản phẩm cho người dùng
        public async Task<List<ProductRecommendationModel>> GetProductRecommendations(string userId)
        {
            var orders = await _recommendationRepository.GetOrderDataAsync(userId);

            var views = await _recommendationRepository.GetViewDataAsync(userId);

            var recommendations = new List<ProductRecommendationModel>();

            // Tổng hợp dữ liệu từ orders và views
            foreach (var order in orders)
            {
                var view = views.FirstOrDefault(v => v.ProductId == order.ProductId);
                var product = await _recommendationRepository.GetProductByIdAsync(order.ProductId);

                recommendations.Add(new ProductRecommendationModel
                {
                    ProductId = order.ProductId,
                    UserId = userId,
                    Label = order.PurchaseCount + (view?.ViewCount ?? 0), 
                    ProductName = product?.ProductName, 
                    Description = product?.Description,
                    ImageUrl = product?.ImageUrl,       
                    Price = product?.Price ?? 0   
                });
            }

            // Thêm những sản phẩm chỉ được xem nhưng không được mua
            foreach (var view in views)
            {
                if (!recommendations.Any(r => r.ProductId == view.ProductId))
                {
                    var product = await _recommendationRepository.GetProductByIdAsync(view.ProductId);

                    recommendations.Add(new ProductRecommendationModel
                    {
                        ProductId = view.ProductId,
                        UserId = userId,
                        Label = view.ViewCount,
                        ProductName = product?.ProductName,
                        Description = product?.Description,
                        ImageUrl = product?.ImageUrl,
                        Price = product?.Price ?? 0
                    });
                }
            }

            // Huấn luyện mô hình từ dữ liệu gợi ý
            var model = TrainModel(recommendations);

            // Lấy các gợi ý sản phẩm từ mô hình
            var topRecommendations = GetRecommendations(model, userId, 5);

            return topRecommendations;
        }


        // Phương thức huấn luyện mô hình
        public ITransformer TrainModel(List<ProductRecommendationModel> data)
        {
            var trainingData = data.Select(d => new ProductRecommendationInput
            {
                UserId = (uint)(Math.Abs(d.UserId.GetHashCode()) % 100000),
                ProductId = (uint)d.ProductId,
                Label = d.Label
            }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            if (dataView.GetRowCount() == 0)
            {
                throw new InvalidOperationException("Không có dữ liệu hợp lệ để huấn luyện.");
            }

            var pipeline = _mlContext.Recommendation().Trainers.MatrixFactorization(
                labelColumnName: nameof(ProductRecommendationInput.Label),
                matrixColumnIndexColumnName: nameof(ProductRecommendationInput.UserId),
                matrixRowIndexColumnName: nameof(ProductRecommendationInput.ProductId),
                numberOfIterations: 20,
                approximationRank: 32);

            return pipeline.Fit(dataView);
        }

        // Phương thức để dự đoán và gợi ý sản phẩm
        public List<ProductRecommendationModel> GetRecommendations(ITransformer model, string userId, int topN)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductRecommendationInput, ProductRecommendationInput>(model);

            var userKey = Convert.ToUInt32(userId.GetHashCode() & 0x7FFFFFFF);
            var allProductIds = _recommendationRepository.GetAllProductIds();
            var recommendations = new List<ProductRecommendationModel>();

            foreach (var productId in allProductIds)
            {
                var prediction = predictionEngine.Predict(new ProductRecommendationInput
                {
                    UserId = userKey,
                    ProductId = productId
                });
                var p = _recommendationRepository.GetProductById((int)productId);
                recommendations.Add(new ProductRecommendationModel
                {
                    ProductId = (int)productId,
                    UserId = userId,
                    Label = prediction.Label,
                    ProductName = p?.ProductName,
                    Description = p?.Description,
                    ImageUrl = p?.ImageUrl,
                    Price = p?.Price ?? 0
                });
            }

            return recommendations.OrderByDescending(r => r.Label).Take(topN).ToList();
        }


        // Phương thức lưu trữ các gợi ý sản phẩm vào database
        //public async Task SaveRecommendations(List<ProductRecommendation> recommendations)
        //{
        //    foreach (var recommendation in recommendations)
        //    {
        //        _context.ProductRecommendations.Add(recommendation);
        //    }

        //    await _context.SaveChangesAsync();
        //}
    }
}
