using Game.Service.Data;
using Game.Service.Data.Models;
using Game.Service.Services;
using Common.Localization;
using Microsoft.EntityFrameworkCore;

namespace Game.Test.Tests
{
    public class KPTest
    {
        private static GameLanguageOptions JapaneseOptions => new() { DefaultLanguage = "ja-JP" };

        [Fact]
        public async Task GenerateSceneDescriptionAsync_ReturnsDescription_WhenSceneExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            context.Scenarios.Add(new Scenario { Id = 1, Name = "Scenario1", Description = "S1" });
            context.SaveChanges();
            context.Scenes.Add(new Scene { Id = 1, Name = "TestScene", Description = "Desc", ScenarioId = 1 });
            await context.SaveChangesAsync();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GenerateSceneDescriptionAsync(1);

            // Assert
            Assert.Contains("Desc", result);
        }

        [Fact]
        public async Task GenerateNpcDialogueAsync_ReturnsNotFound_WhenNoNpc()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GenerateNpcDialogueAsync(999);

            // Assert
            Assert.Contains("登場NPCはいません", result);
        }

        [Fact]
        public async Task GenerateNpcDialogueAsync_ReturnsDialogue_WhenNpcExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            context.Scenarios.Add(new Scenario { Id = 1, Name = "Scenario1", Description = "S1" });
            context.SaveChanges();
            context.Scenes.Add(new Scene { Id = 1, Name = "TestScene", Description = "Desc", ScenarioId = 1 });
            context.SaveChanges();
            context.NonPlayerCharacters.Add(new NonPlayerCharacter { Id = 1, Name = "NPC", Role = "Villager", LastKnownSceneId = 1, IsActive = true });
            await context.SaveChangesAsync();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GenerateNpcDialogueAsync(1);

            // Assert
            Assert.Contains("NPC", result);
        }

        [Fact]
        public async Task GenerateRandomEventAsync_ReturnsEvent_WhenExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            context.Scenarios.Add(new Scenario { Id = 1, Name = "Scenario1", Description = "S1" });
            context.SaveChanges();
            context.Scenes.Add(new Scene { Id = 1, Name = "TestScene", Description = "Desc", ScenarioId = 1 });
            context.SaveChanges();
            context.EventIntensities.Add(new EventIntensity { Id = 1, Name = "Normal", Description = "Normal intensity" });
            context.SaveChanges();
            context.RandomEvents.Add(new RandomEvent { Id = 1, Name = "嵐", SceneId = 1, Description = "Random event!", EventIntensityId = 1, IsActive = true });
            await context.SaveChangesAsync();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GenerateRandomEventAsync(1);

            // Assert
            Assert.Contains("ランダムイベント", result);
        }

        [Fact]
        public async Task SuggestChecksAndDifficultiesAsync_ReturnsNotFound_WhenNoScene()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.SuggestChecksAndDifficultiesAsync(999);

            // Assert
            Assert.Contains("判定提案はありません", result);
        }

        [Fact]
        public async Task SuggestChecksAndDifficultiesAsync_ReturnsSuggestion_WhenSceneExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            context.Scenarios.Add(new Scenario { Id = 1, Name = "Scenario1", Description = "S1" });
            context.SaveChanges();
            context.Scenes.Add(new Scene { Id = 2, Name = "Scene2", Description = "Desc2", ScenarioId = 1 });
            await context.SaveChangesAsync();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.SuggestChecksAndDifficultiesAsync(2);

            // Assert
            Assert.Contains("判定提案はありません", result);
        }

        [Fact]
        public async Task GetGameProgressSuggestionsAsync_ReturnsSuggestion_WhenRecordsExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            context.Scenarios.Add(new Scenario { Id = 1, Name = "Scenario1", Description = "S1" });
            context.SaveChanges();
            context.GameRecords.Add(new GameRecords { Id = 1, ScenarioId = 1, Description = "Progress!", ActionTime = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GetGameProgressSuggestionsAsync(1);

            // Assert
            Assert.Contains("Progress", result);
        }

        [Fact]
        public async Task GenerateSceneDescriptionAsync_ReturnsNotFound_WhenNoScene()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GenerateSceneDescriptionAsync(999);

            // Assert
            Assert.Contains("見つかりません", result);
        }

        [Fact]
        public async Task GenerateRandomEventAsync_ReturnsNotFound_WhenNoScene()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GenerateRandomEventAsync(999);

            // Assert
            Assert.Contains("見つかりません", result);
        }

        [Fact]
        public async Task GetGameProgressSuggestionsAsync_ReturnsNoRecords()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            using var context = new TrpgDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            var service = new KPService(context, JapaneseOptions);

            // Act
            var result = await service.GetGameProgressSuggestionsAsync(999);

            // Assert
            Assert.Contains("直近の記録はありません", result);
        }
    }
}
