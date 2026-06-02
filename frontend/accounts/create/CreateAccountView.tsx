import CreateAccountForm from "@/accounts/create/CreateAccountForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Component that displays the create account view.
 */
const CreateAccountView = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const { data: accountingPeriods } = await apiClient.GET(
    "/accounting-periods/open",
  );
  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  return <CreateAccountForm accountingPeriods={accountingPeriods} />;
};

export default CreateAccountView;
