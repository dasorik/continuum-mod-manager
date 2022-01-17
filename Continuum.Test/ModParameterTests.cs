using NUnit.Framework;
using Continuum.Core.Models;

namespace Continuum.Core.Test
{
	public class ModParameterTests
	{
		public ModInstallInfo GetGenericModInfo()
		{
			return new ModInstallInfo
			(
				config: new ModConfiguration()
				{
					ModID = "test-id"
				},
				modParameters: new System.Collections.Generic.Dictionary<string, string>
				{
					{ "test-setting", "Test Value" },
					{ "test-setting-bool", "true" },
					{ "test-setting-equal", "Test Value" },
					{ "test-setting-int", "56" },
					{ "test-setting-decimal", "56.67" }
				},
				integrationParameters: new System.Collections.Generic.Dictionary<string, string>
				{
					{ "test-setting", "Test Value" },
					{ "test-setting-bool", "true" },
					{ "test-setting-equal", "Test Value" },
					{ "test-setting-int", "56" },
					{ "test-setting-decimal", "56.67" }
				},
				integration: new GameIntegration() {
					IntegrationID = "test.integration"
				}
			);
		}

		
		[Test]
		public void InvalidPredicate_EmptyString()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void InvalidPredicate_Null()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch(null);

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void InvalidPredicate_InvalidString_1()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("\"Test\" = \"Test\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void InvalidPredicate_InvalidString_2()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("true = \"Test\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void InvalidPredicate_InvalidString_3()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("45.67 = \"Test\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}


		// String Comparisons

		[Test]
		public void ModParameter_String_Equals_String_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal = \"Test Value\"");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_String_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal = \"Test Value2\"");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_NotEquals_String_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal != \"Test Value2\"");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_NotEquals_String_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal != \"Test Value\"");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_LessThanOrEqualTo_String()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal <= \"Test Value\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_GreaterThanOrEqualTo_String()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal >= \"Test Value\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_LessThan_String()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal < \"Test Value\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_GreaterThan_String()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal > \"Test Value\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_Parameter_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal = $MOD.test-setting");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_InvalidParameter()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-equal = $MOD.test-setting6");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_Parameter_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting = $MOD.test-setting-bool");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_IntegrationParameter_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting = $INTEGRATION.test-setting");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_InvalidIntegrationParameter()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting = $INTEGRATION.test-setting6");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_String_Equals_IntegerationParameter_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting = $INTEGRATION.test-setting-bool");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}


		// Decimal Comparisons

		[Test]
		public void ModParameter_Decimal_Equals_Decimal_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = 56.67");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_Decimal_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = 56.68");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_NotEquals_Decimal_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal != 56.65");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_NotEquals_Decimal_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal != 56.67");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_LessThanOrEqualTo_Decimal_GreaterThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal <= 56.68");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_LessThanOrEqualTo_Decimal_EqualTo()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal <= 56.67");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_LessThanOrEqualTo_Decimal_LessThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal <= 56.65");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_GreaterThanOrEqualTo_Decimal_GreaterThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal >= 56.68");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_GreaterThanOrEqualTo_Decimal_EqualTo()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal >= 56.67");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_GreaterThanOrEqualTo_Decimal_LessThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal >= 56.65");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_LessThan_Decimal_GreaterThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal < 56.68");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_LessThan_Decimal_EqualTo()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal < 56.67");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_LessThan_Decimal_LessThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal < 56.65");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_GreaterThan_Decimal_GreaterThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal > 56.68");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_GreaterThan_Decimal_EqualTo()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal > 56.67");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_GreaterThan_Decimal_LessThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal > 56.65");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_Parameter_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = $MOD.test-setting-decimal");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_InvalidParameter()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = $MOD.test-setting6");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_Parameter_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = $MOD.test-setting-bool");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_IntegrationParameter_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = $INTEGRATION.test-setting-decimal");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_InvalidIntegrationParameter()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = $INTEGRATION.test-setting6");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_Decimal_Equals_IntegerationParameter_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$MOD.test-setting-decimal = $INTEGRATION.test-setting-bool");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_Equals()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID = \"test.integration\"");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_Equals_Invalid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID = \"test.id\"");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_Valid_NotEquals()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID != \"test.id\"");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID = \"Test.ID2\"");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_LessThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID < \"Test.ID2\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_LessThanOrEqualTo()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID <= \"Test.ID2\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_GreaterThan()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID > \"Test.ID2\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_GreaterThanOrEqualTo()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID >= \"Test.ID2\"");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_LessThan_Number()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID < 12");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_LessThanOrEqualTo_Number()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID <= 12");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_GreaterThan_Number()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID > 12");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_GreaterThanOrEqualTo_Number()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID >= 12");

			Assert.AreEqual(PredicateMatchType.InvalidPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Invalid_Number()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID = 12");

			Assert.AreEqual(PredicateMatchType.DoesNotMatchPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Valid_Number()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID != 12");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

		[Test]
		public void ModParameter_IntegrationID_NotEquals_Valid()
		{
			var info = GetGenericModInfo();
			var matchesPredicate = info.PredicateMatch("$INTEGRATION_ID != \"Test.ID3\"");

			Assert.AreEqual(PredicateMatchType.MatchesPredicate, matchesPredicate);
		}

	}
}