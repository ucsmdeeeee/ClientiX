using ClientiX.Application.Interfaces;
using ClientiX.Domain.Enums;
using ClientiX.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientiX.Application.Services
{
    public class SubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IMasterUserRepository _userRepository;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            IMasterUserRepository userRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(long telegramUserId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramUserId);
            return user?.SubscriptionId != null
                ? await _subscriptionRepository.GetActiveByUserIdAsync(telegramUserId)
                : null;
        }

        public async Task ActivateTrialSubscriptionAsync(long telegramUserId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramUserId);
            if (user == null) return;

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                PlanType = SubscriptionPlan.Trial7Days,
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            await _subscriptionRepository.CreateAsync(subscription);
            user.SubscriptionId = subscription.Id;
            await _userRepository.CreateOrUpdateAsync(user);
        }

        public async Task<bool> IsSubscriptionActiveAsync(long telegramUserId)
        {
            var subscription = await GetActiveSubscriptionAsync(telegramUserId);
            return subscription != null && subscription.IsActive;
        }

        public Dictionary<SubscriptionPlan, (decimal Price, int Days)> GetTariffs()
        {
            return new()
            {
                [SubscriptionPlan.Trial7Days] = (0, 7),
                [SubscriptionPlan.Monthly500] = (500, 30),
                [SubscriptionPlan.Quarterly1200] = (1200, 90),
                [SubscriptionPlan.HalfYear2000] = (2000, 180)
            };
        }
    }
}
