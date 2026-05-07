import type {
  DeleteFundViewParams,
  DeleteFundViewSearchParams,
} from "@/funds/DeleteFundView";
import type { FundViewParams, FundViewSearchParams } from "@/funds/FundView";
import type {
  UpdateFundViewParams,
  UpdateFundViewSearchParams,
} from "@/funds/UpdateFundView";
import type { CreateFundViewSearchParams } from "@/funds/CreateFundView";
import type { FundsViewSearchParams } from "@/funds/FundsView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to funds.
 */
const routes = {
  index: (searchParams: FundsViewSearchParams): Route =>
    `/funds?${objectToSearchParams(searchParams).toString()}` as Route,
  create: (searchParams: CreateFundViewSearchParams): Route =>
    `/funds/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (params: FundViewParams, searchParams: FundViewSearchParams): Route =>
    `/funds/${params.id}?${objectToSearchParams(searchParams).toString()}` as Route,
  update: (
    params: UpdateFundViewParams,
    searchParams: UpdateFundViewSearchParams,
  ): Route =>
    `/funds/${params.id}/update?${objectToSearchParams(searchParams).toString()}` as Route,
  delete: (
    params: DeleteFundViewParams,
    searchParams: DeleteFundViewSearchParams,
  ): Route =>
    `/funds/${params.id}/delete?${objectToSearchParams(searchParams).toString()}` as Route,
};

export default routes;
