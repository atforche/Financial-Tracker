import type { components } from "@/framework/data/api";
import getApiClient from "@/framework/data/getApiClient";

type OnboardingEligibility =
  components["schemas"]["OnboardingEligibilityModel"];

/**
 * Gets whether the system still requires onboarding.
 */
const getOnboardingEligibility =
  async function (): Promise<OnboardingEligibility> {
    const client = getApiClient();
    const { data } = await client.GET("/onboarding/eligibility");
    if (typeof data === "undefined") {
      throw new Error("Failed to fetch onboarding eligibility.");
    }

    return data;
  };

export default getOnboardingEligibility;
