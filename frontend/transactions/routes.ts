import type {
  DeleteTransactionViewParams,
  DeleteTransactionViewSearchParams,
} from "@/transactions/DeleteTransactionView";
import type {
  PostTransactionViewParams,
  PostTransactionViewSearchParams,
} from "@/transactions/PostTransactionView";
import type {
  TransactionViewParams,
  TransactionViewSearchParams,
} from "@/transactions/TransactionView";
import type {
  UnpostTransactionViewParams,
  UnpostTransactionViewSearchParams,
} from "@/transactions/UnpostTransactionView";
import type {
  UpdateTransactionViewParams,
  UpdateTransactionViewSearchParams,
} from "@/transactions/UpdateTransactionView";
import type { CreateTransactionViewSearchParams } from "@/transactions/CreateTransactionView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to transactions.
 */
const routes = {
  create: (searchParams: CreateTransactionViewSearchParams): Route =>
    `/transactions/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (
    params: TransactionViewParams,
    searchParams: TransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}?${objectToSearchParams(searchParams).toString()}` as Route,
  update: (
    params: UpdateTransactionViewParams,
    searchParams: UpdateTransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}/update?${objectToSearchParams(searchParams).toString()}` as Route,
  post: (
    params: PostTransactionViewParams,
    searchParams: PostTransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}/post?${objectToSearchParams(searchParams).toString()}` as Route,
  unpost: (
    params: UnpostTransactionViewParams,
    searchParams: UnpostTransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}/unpost?${objectToSearchParams(searchParams).toString()}` as Route,
  delete: (
    params: DeleteTransactionViewParams,
    searchParams: DeleteTransactionViewSearchParams,
  ): Route =>
    `/transactions/${params.id}/delete?${objectToSearchParams(searchParams).toString()}` as Route,
};

export default routes;
