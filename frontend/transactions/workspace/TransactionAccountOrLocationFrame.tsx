import type { Account, AccountIdentifier } from "@/accounts/types";
import { Box, Stack, Typography } from "@mui/material";
import type {
  Transaction,
  TransactionAccountDraft,
} from "@/transactions/transaction";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionAccountFrame from "@/transactions/workspace/TransactionAccountFrame";

/**
 * Props for the TransactionAccountOrLocationFrame component.
 */
interface TransactionAccountOrLocationFrameProps {
  readonly accounts: Account[];
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount:
    ((account: TransactionAccountDraft | null) => void) | null;
  readonly accountCaption?: string;
  readonly locationCaption: string;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly balanceChange?: number | null;
}

/**
 * Displays a shared framed account-or-location entry block for transaction forms.
 */
const TransactionAccountOrLocationFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  accountCaption = "Account",
  locationCaption,
  location,
  setLocation,
  accountFilter = null,
  balanceChange = null,
}: TransactionAccountOrLocationFrameProps): JSX.Element {
  return (
    <Stack
      direction={{ xs: "column", md: "row" }}
      spacing={2}
      alignItems={{ xs: "stretch", md: "flex-start" }}
    >
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <TransactionAccountFrame
          accounts={accounts}
          transaction={transaction}
          account={account}
          setAccount={setAccount}
          accountFilter={accountFilter}
          label={accountCaption}
          balanceChange={balanceChange}
        />
      </Box>
      <Typography
        align="center"
        variant="subtitle1"
        sx={{ flexShrink: 0, minWidth: { md: 40 }, pt: { md: 2 } }}
      >
        Or
      </Typography>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <StringEntryField
          label={locationCaption}
          value={location}
          setValue={account === null ? setLocation : null}
        />
      </Box>
    </Stack>
  );
};

export default TransactionAccountOrLocationFrame;
