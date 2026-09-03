import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import {
  ComboBoxEntryField,
  type ComboBoxOption,
} from "@/framework/forms/ComboBoxEntryField";
import type { Location, LocationDraft } from "@/locations/types";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import type { Transaction } from "@/transactions/types";
import { getSelectedTransactionAccountDraft } from "@/transactions/workspace/accountBalanceEventDraft";
import { useLocations } from "@/locations/LocationProvider";

/**
 * Props for the TransactionAccountOrLocationFrame component.
 */
interface TransactionAccountOrLocationFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly accountCaption?: string;
  readonly locationCaption: string;
  readonly entryCaption?: string;
  readonly locations?: readonly Location[] | undefined;
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly balanceChange?: number | null;
  readonly readOnly?: boolean;
  readonly autoFocus?: boolean;
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
  entryCaption = "Destination",
  locations,
  location,
  setLocation,
  accountFilter = null,
  balanceChange = null,
  readOnly = false,
  autoFocus = false,
}: TransactionAccountOrLocationFrameProps): JSX.Element {
  const contextLocations = useLocations();
  const availableLocations = locations ?? contextLocations;
  const hasAccount = account !== null;
  const hasLocation = (location?.name ?? "").trim() !== "";

  if (readOnly && hasAccount && !hasLocation) {
    return (
      <AccountBalanceEventFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={null}
        accountFilter={accountFilter}
        label={accountCaption}
        balanceChange={balanceChange}
      />
    );
  }

  if (readOnly && !hasAccount) {
    return (
      <StringEntryField
        label={locationCaption}
        value={location?.name ?? null}
        setValue={null}
      />
    );
  }

  type DestinationOption =
    | { readonly kind: "account"; readonly account: Account }
    | { readonly kind: "location"; readonly location: Location }
    | { readonly kind: "new-location"; readonly name: string };

  const destinationOptions: ComboBoxOption<DestinationOption>[] = [
    ...accounts
      .filter((candidate) => (accountFilter ? accountFilter(candidate) : true))
      .map((candidate) => ({
        label: candidate.name,
        secondaryLabel: "Account",
        value: { kind: "account", account: candidate } as const,
      })),
    ...[...availableLocations]
      .sort((left, right) => left.name.localeCompare(right.name))
      .map((candidate) => ({
        label: candidate.name,
        secondaryLabel: "Location",
        value: { kind: "location", location: candidate } as const,
      })),
  ];

  const selectedLocation =
    location === null
      ? null
      : (availableLocations.find((candidate) => candidate.id === location.id) ??
        null);
  const selectedAccount =
    account === null
      ? null
      : (accounts.find((candidate) => candidate.id === account.accountId) ??
        null);
  const selectedDestination: ComboBoxOption<DestinationOption> | null =
    account === null
      ? selectedLocation === null
        ? location === null
          ? null
          : {
              label: location.name,
              value: { kind: "new-location", name: location.name },
            }
        : {
            label: selectedLocation.name,
            secondaryLabel: "Location",
            value: { kind: "location", location: selectedLocation },
          }
      : selectedAccount === null
        ? null
        : {
            label: selectedAccount.name,
            secondaryLabel: "Account",
            value: {
              kind: "account",
              account: selectedAccount,
            },
          };

  const setDestination = (option: DestinationOption | null): void => {
    if (option?.kind === "account") {
      setAccount?.(
        getSelectedTransactionAccountDraft(
          accounts,
          option.account,
          account,
          balanceChange,
        ),
      );
      setLocation?.(null);
    } else if (option?.kind === "location") {
      setAccount?.(null);
      setLocation?.({ id: option.location.id, name: option.location.name });
    } else if (option?.kind === "new-location") {
      setAccount?.(null);
      setLocation?.({ id: null, name: option.name });
    } else {
      setAccount?.(null);
      setLocation?.(null);
    }
  };

  const destinationEntryField = (
    <ComboBoxEntryField<DestinationOption>
      label={entryCaption}
      options={destinationOptions}
      value={selectedDestination}
      autoFocus={autoFocus}
      setValue={(newValue): void => {
        setDestination(newValue?.value ?? null);
      }}
      createOption={(inputValue) => {
        const name = inputValue.trim();
        const exists = availableLocations.some(
          (candidate) =>
            candidate.name.localeCompare(name, undefined, {
              sensitivity: "accent",
            }) === 0,
        );
        return name === "" || exists
          ? null
          : {
              label: `Add new location "${name}"`,
              value: { kind: "new-location", name },
            };
      }}
      isOptionEqualToValue={(left, right) =>
        left.value?.kind === right.value?.kind &&
        (left.value?.kind === "account"
          ? right.value?.kind === "account" &&
            left.value.account.id === right.value.account.id
          : left.value?.kind === "location"
            ? right.value?.kind === "location" &&
              left.value.location.id === right.value.location.id
            : left.label === right.label)
      }
    />
  );

  const accountOrLocationContent = (
    <Stack spacing={1}>
      {destinationEntryField}
      {hasAccount ? (
        <AccountBalanceEventFrame
          accounts={accounts}
          transaction={transaction}
          account={account}
          setAccount={setAccount}
          accountFilter={accountFilter}
          label={accountCaption}
          balanceChange={balanceChange}
          showAccountEntry={false}
        />
      ) : null}
    </Stack>
  );

  return accountOrLocationContent;
};

export default TransactionAccountOrLocationFrame;
