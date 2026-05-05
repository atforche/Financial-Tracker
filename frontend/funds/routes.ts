import type {
  DeleteFundViewParams,
  DeleteFundViewSearchParams,
} from "@/funds/delete/DeleteFundView";
import type {
  FundDetailViewParams,
  FundDetailViewSearchParams,
} from "@/funds/detail/FundDetailView";
import type {
  UpdateFundViewParams,
  UpdateFundViewSearchParams,
} from "@/funds/update/UpdateFundView";
import type { CreateFundViewSearchParams } from "@/funds/create/CreateFundView";
import type { FundIndexViewSearchParams } from "@/funds/index/FundIndexView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to funds.
 */
const routes = {
  index: (searchParams: FundIndexViewSearchParams): Route =>
    `/funds?${objectToSearchParams(searchParams).toString()}` as Route,
  create: (searchParams: CreateFundViewSearchParams): Route =>
    `/funds/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (
    params: FundDetailViewParams,
    searchParams: FundDetailViewSearchParams,
  ): Route =>
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
