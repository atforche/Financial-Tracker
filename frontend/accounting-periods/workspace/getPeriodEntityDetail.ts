import type { components } from "@/framework/data/api";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * An accounting period's detail, with the API client that loaded it for the page's further requests.
 */
interface PeriodEntityDetail {
  readonly apiClient: Awaited<ReturnType<typeof createApiClient>>;
  readonly detail: components["schemas"]["AccountingPeriodDetailModel"];
}

/**
 * Loads the accounting period behind an account or fund's period page, leaving for the period list when it's missing.
 */
const getPeriodEntityDetail = async function (
  accountingPeriodId: string,
): Promise<PeriodEntityDetail> {
  const apiClient = await createApiClient();
  const detailResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/detail",
    { params: { path: { accountingPeriodId } } },
  );
  if (detailResponse.response.status === 404) {
    redirect(routes.workspace({}));
  }
  const detail = unwrapApiResponse(
    detailResponse,
    "Failed to fetch accounting period",
  );
  return { apiClient, detail };
};

export default getPeriodEntityDetail;
