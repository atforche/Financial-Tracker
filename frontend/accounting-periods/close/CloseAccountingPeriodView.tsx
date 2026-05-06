import {
  type CloseAccountingPeriodViewParams,
  buildCloseAccountingPeriodNavigationContext,
} from "@/accounting-periods/close/closeAccountingPeriodNavigationContext";
import CloseAccountingPeriodForm from "@/accounting-periods/close/CloseAccountingPeriodForm";
import type { JSX } from "react";

/**
 * Props for the CloseAccountingPeriodView component.
 */
interface CloseAccountingPeriodViewProps {
  readonly params: Promise<CloseAccountingPeriodViewParams>;
}

/**
 * Component that displays the close accounting period view.
 */
const CloseAccountingPeriodView = async function ({
  params,
}: CloseAccountingPeriodViewProps): Promise<JSX.Element> {
  const navigationContext = await buildCloseAccountingPeriodNavigationContext(
    await params,
  );
  return <CloseAccountingPeriodForm navigationContext={navigationContext} />;
};

export type { CloseAccountingPeriodViewParams };
export default CloseAccountingPeriodView;
