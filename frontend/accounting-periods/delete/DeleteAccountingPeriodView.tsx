import { type DeleteAccountingPeriodParams, buildDeleteAccountingPeriodNavigationContext } from "@/accounting-periods/delete/deleteAccountingPeriodNavigationContext";
import DeleteAccountingPeriodForm from "@/accounting-periods/delete/DeleteAccountingPeriodForm";
import type { JSX } from "react";

/**
 * Props for the DeleteAccountingPeriodView component.
 */
interface DeleteAccountingPeriodViewProps {
  readonly params: Promise<DeleteAccountingPeriodParams>;
}

/**
 * Component that displays the delete accounting period view.
 */
const DeleteAccountingPeriodView = async function ({
  params,
}: DeleteAccountingPeriodViewProps): Promise<JSX.Element> {
  const navigationContext = await buildDeleteAccountingPeriodNavigationContext(
    await params,
  );
  return <DeleteAccountingPeriodForm navigationContext={navigationContext} />;
};

export default DeleteAccountingPeriodView;
