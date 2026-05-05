import type {
  AccountDetailViewParams,
  AccountDetailViewSearchParams,
} from "@/accounts/detail/AccountDetailView";
import type {
  DeleteAccountViewParams,
  DeleteAccountViewSearchParams,
} from "@/accounts/delete/DeleteAccountView";
import type {
  UpdateAccountViewParams,
  UpdateAccountViewSearchParams,
} from "@/accounts/update/UpdateAccountView";
import type { AccountIndexViewSearchParams } from "@/accounts/index/AccountIndexView";
import type { CreateAccountViewSearchParams } from "@/accounts/create/CreateAccountView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to accounts.
 */
const routes = {
  index: (searchParams: AccountIndexViewSearchParams): Route =>
    `/accounts?${objectToSearchParams(searchParams).toString()}` as Route,
  create: (searchParams: CreateAccountViewSearchParams): Route =>
    `/accounts/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (
    params: AccountDetailViewParams,
    searchParams: AccountDetailViewSearchParams,
  ): Route =>
    `/accounts/${params.id}?${objectToSearchParams(searchParams).toString()}` as Route,
  update: (
    params: UpdateAccountViewParams,
    searchParams: UpdateAccountViewSearchParams,
  ): Route =>
    `/accounts/${params.id}/update?${objectToSearchParams(searchParams).toString()}` as Route,
  delete: (
    params: DeleteAccountViewParams,
    searchParams: DeleteAccountViewSearchParams,
  ): Route =>
    `/accounts/${params.id}/delete?${objectToSearchParams(searchParams).toString()}` as Route,
};

export default routes;
