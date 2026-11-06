"use server";

import createApiClient from "@/framework/data/createApiClient";

/**
 * Loads the financial institutions already in use, as suggestions that the account form can do without.
 */
const getFinancialInstitutions = async function (): Promise<string[]> {
  try {
    const apiClient = await createApiClient();
    const { data } = await apiClient.GET("/accounts/financial-institutions");
    return data?.items ?? [];
  } catch {
    return [];
  }
};

export default getFinancialInstitutions;
