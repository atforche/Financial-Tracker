import CreateAccountingPeriodForm from "@/accounting-periods/CreateAccountingPeriodForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Component that displays the create Accounting Period page.
 */
const CreateAccountingPeriodView = async function (): Promise<JSX.Element> {
  const client = getApiClient();
  const { data: accountingPeriods } = await client.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  return (
    <CreateAccountingPeriodForm
      isInOnboardingMode={accountingPeriods.items.length === 0}
    />
  );
};

export default CreateAccountingPeriodView;
