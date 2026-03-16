import CreateAccountingPeriodFundForm from "@/app/accounting-periods/[id]/fund/create/CreateAccountingPeriodFundForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Component that displays the create fund view.
 */
const Page = async function (props: {
  readonly params: Promise<{ id: string }>;
}): Promise<JSX.Element> {
  const { id } = await props.params;
  const apiClient = getApiClient();
  const { data, error } = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );
  if (typeof data === "undefined") {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${error.detail}`,
    );
  }

  return <CreateAccountingPeriodFundForm accountingPeriod={data} />;
};

export default Page;
