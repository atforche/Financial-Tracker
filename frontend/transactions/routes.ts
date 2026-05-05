import type {
  DeleteTransactionViewParams,
  DeleteTransactionViewSearchParams,
} from "@/transactions/delete/DeleteTransactionView";
import type {
  PostTransactionViewParams,
  PostTransactionViewSearchParams,
} from "@/transactions/post/PostTransactionView";
import type {
  TransactionDetailViewParams,
  TransactionDetailViewSearchParams,
} from "@/transactions/detail/TransactionDetailView";
import type {
  UnpostTransactionViewParams,
  UnpostTransactionViewSearchParams,
} from "@/transactions/unpost/UnpostTransactionView";
import type {
  UpdateTransactionViewParams,
  UpdateTransactionViewSearchParams,
} from "@/transactions/update/UpdateTransactionView";
import type { CreateTransactionViewSearchParams } from "@/transactions/create/CreateTransactionView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to transactions.
 */
const routes = {
  create: (searchParams: CreateTransactionViewSearchParams): Route =>
    `/transactions/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (
    params: TransactionDetailViewParams,
    searchParams: TransactionDetailViewSearchParams,
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
