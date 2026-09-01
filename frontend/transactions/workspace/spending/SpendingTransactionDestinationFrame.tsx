import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import {
  Box,
  Chip,
  Collapse,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import {
  type FundAssignmentDraft,
  getAssignedFundAmount,
  getExplicitFundAssignments,
  getRemainingFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import { type JSX, useId, useState } from "react";
import type { Location, LocationDraft } from "@/locations/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ExpandMore from "@mui/icons-material/ExpandMore";
import type { FrameColor } from "@/framework/view/Frame";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import InsetFrame from "@/framework/view/InsetFrame";
import SpendingFundAssignmentPlanner from "@/funds/assignmentPlanner/SpendingFundAssignmentPlanner";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the SpendingTransactionDestinationFrame component.
 */
interface SpendingTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly locations?: readonly Location[] | undefined;
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments?: FundAssignmentDraft[];
  readonly filter?: ((account: Account) => boolean) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly fundAssignmentsValid?: boolean;
  readonly readOnly?: boolean;
}

const emptyFundAssignments: FundAssignmentDraft[] = [];

/**
 * Displays a destination frame for one spending destination.
 */
const SpendingTransactionDestinationFrame = function ({
  index,
  accounts,
  funds,
  fundGoals,
  transaction = null,
  account,
  setAccount,
  location,
  locations,
  setLocation,
  amount,
  setAmount,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments = emptyFundAssignments,
  filter = null,
  onRemove = null,
  color = "info",
  fundAssignmentsValid = true,
  readOnly = false,
}: SpendingTransactionDestinationFrameProps): JSX.Element {
  const [fundAssignmentsExpanded, setFundAssignmentsExpanded] = useState(false);
  const fundAssignmentsDetailsId = useId();
  const assignedAmount = getAssignedFundAmount(fundAssignments);
  const remainingAmount = getRemainingFundAmount(amount, fundAssignments);
  const explicitFundAssignmentCount =
    getExplicitFundAssignments(fundAssignments).length;
  const shouldCollapseFundAssignments =
    readOnly && explicitFundAssignmentCount > 1;

  const fundAssignmentsContent = (
    <SpendingFundAssignmentPlanner
      funds={funds}
      fundGoals={fundGoals}
      totalAmountToAssign={amount}
      fundAssignments={fundAssignments}
      setFundAssignments={readOnly ? null : setFundAssignments}
      baselineFundAssignments={baselineFundAssignments}
      readOnly={readOnly}
    />
  );

  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      headerContentInline
      onRemove={readOnly ? null : onRemove}
      color={fundAssignmentsValid ? color : "error"}
      headerContent={
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
          <Chip
            color={remainingAmount === 0 ? "success" : "error"}
            label={`Unassigned ${formatCurrency(remainingAmount ?? 0)}`}
          />
        </Stack>
      }
    >
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        alignItems={{ xs: "stretch", sm: "flex-start" }}
      >
        <Box sx={{ flex: { sm: "1 1 auto" }, minWidth: 0 }}>
          <TransactionAccountOrLocationFrame
            accounts={accounts}
            transaction={transaction}
            account={account}
            setAccount={readOnly ? null : setAccount}
            accountCaption="Destination Account"
            locationCaption="Destination Location"
            entryCaption="Destination"
            locations={locations}
            location={location}
            setLocation={readOnly ? null : setLocation}
            accountFilter={filter}
            balanceChange={amount}
            readOnly={readOnly}
          />
        </Box>
        <CurrencyEntryField
          label="Destination Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{ width: { xs: "100%", sm: 220 } }}
        />
      </Stack>
      {shouldCollapseFundAssignments ? (
        <InsetFrame>
          <Stack
            direction="row"
            alignItems="center"
            justifyContent="space-between"
          >
            <Typography variant="subtitle1">
              Fund Assignments ({explicitFundAssignmentCount})
            </Typography>
            <IconButton
              size="small"
              onClick={() => {
                setFundAssignmentsExpanded((expanded) => !expanded);
              }}
              aria-label={`${fundAssignmentsExpanded ? "Collapse" : "Expand"} Fund Assignments`}
              aria-expanded={fundAssignmentsExpanded}
              aria-controls={fundAssignmentsDetailsId}
              sx={{
                p: 0.5,
                transform: fundAssignmentsExpanded
                  ? "rotate(180deg)"
                  : "rotate(0deg)",
                transition: "transform 0.3s ease-in-out",
              }}
            >
              <ExpandMore />
            </IconButton>
          </Stack>
          <Collapse
            id={fundAssignmentsDetailsId}
            in={fundAssignmentsExpanded}
            timeout="auto"
            unmountOnExit
          >
            <Box sx={{ pt: 1.5 }}>{fundAssignmentsContent}</Box>
          </Collapse>
        </InsetFrame>
      ) : (
        fundAssignmentsContent
      )}
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionDestinationFrame;
