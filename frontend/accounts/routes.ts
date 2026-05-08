import type {
  AccountViewParams,
  AccountViewSearchParams,
} from "@/accounts/AccountView";
import type {
  DeleteAccountViewParams,
  DeleteAccountViewSearchParams,
} from "@/accounts/DeleteAccountView";
import type {
  UpdateAccountViewParams,
  UpdateAccountViewSearchParams,
} from "@/accounts/UpdateAccountView";
import type { AccountsViewSearchParams } from "@/accounts/AccountsView";
import type { CreateAccountViewSearchParams } from "@/accounts/CreateAccountView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to accounts.
 */
const routes = {
  index: (searchParams: AccountsViewSearchParams): Route =>
    `/accounts?${objectToSearchParams(searchParams).toString()}` as Route,
  create: (searchParams: CreateAccountViewSearchParams): Route =>
    `/accounts/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (
    params: AccountViewParams,
    searchParams: AccountViewSearchParams,
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
