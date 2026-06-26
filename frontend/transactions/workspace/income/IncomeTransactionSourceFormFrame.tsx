import type { Account, AccountIdentifier } from "@/accounts/types";
import { AddCircleOutline, DeleteOutline } from "@mui/icons-material";
import { Box, Button, IconButton, Stack, Typography } from "@mui/material";
import {
  type IncomeDeductionDraft,
  type IncomeLineDraft,
  createEmptyDeduction,
  createEmptyLine,
} from "@/transactions/workspace/income/helpers";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

/**
 * Props for the IncomeTransactionSourceFrame component.
 */
interface IncomeTransactionSourceFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly setIncomeLines: (incomeLines: IncomeLineDraft[]) => void;
  readonly incomeDeductions: IncomeDeductionDraft[];
  readonly setIncomeDeductions: (
    incomeDeductions: IncomeDeductionDraft[],
  ) => void;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays the source frame for an income transaction.
 */
const IncomeTransactionSourceFrame = function ({
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  incomeLines,
  setIncomeLines,
  incomeDeductions,
  setIncomeDeductions,
  accountFilter = null,
}: IncomeTransactionSourceFrameProps): JSX.Element {
  const updateItem = function <
    T extends IncomeLineDraft | IncomeDeductionDraft,
  >(
    items: T[],
    setItems: (nextItems: T[]) => void,
    index: number,
    recipe: (current: T) => T,
  ): void {
    setItems(
      items.map((item, itemIndex) =>
        itemIndex === index ? recipe(item) : item,
      ),
    );
  };

  const renderLinesAndDeductions = function <
    T extends IncomeLineDraft | IncomeDeductionDraft,
  >(
    title: string,
    description: string,
    items: T[],
    setItems: (nextItems: T[]) => void,
    addLabel: string,
    allowEmpty: boolean,
    createEmptyItem: () => T,
  ): JSX.Element {
    return (
      <Stack spacing={1.5}>
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">{title}</Typography>
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        </Stack>
        {items.map((item, index) => (
          <Box
            key={`${title}-${index}`}
            sx={{
              display: "grid",
              gap: 1.5,
              gridTemplateColumns: {
                xs: "1fr",
                md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
              },
              alignItems: "start",
            }}
          >
            <StringEntryField
              label="Description"
              value={item.description}
              setValue={(nextDescription) => {
                updateItem(items, setItems, index, (current) => ({
                  ...current,
                  description: nextDescription,
                }));
              }}
            />
            <CurrencyEntryField
              label="Amount"
              value={item.amount}
              setValue={(nextAmount) => {
                updateItem(items, setItems, index, (current) => ({
                  ...current,
                  amount: nextAmount,
                }));
              }}
            />
            <Box
              sx={{
                display: "flex",
                justifyContent: { xs: "flex-end", md: "center" },
                pt: { xs: 0, md: 1.25 },
              }}
            >
              <IconButton
                color="error"
                onClick={() => {
                  const nextItems = items.filter(
                    (_, itemIndex) => itemIndex !== index,
                  );
                  if (nextItems.length === 0 && !allowEmpty) {
                    setItems([createEmptyItem()]);
                    return;
                  }
                  setItems(nextItems);
                }}
              >
                <DeleteOutline />
              </IconButton>
            </Box>
          </Box>
        ))}
        <Button
          variant="outlined"
          startIcon={<AddCircleOutline />}
          onClick={() => {
            setItems([...items, createEmptyItem()]);
          }}
          sx={{ alignSelf: "flex-start" }}
        >
          {addLabel}
        </Button>
      </Stack>
    );
  };

  return (
    <TransactionFrame
      title="Income Source"
      description="Choose where the income originated and capture the gross lines and deductions that produce the net transaction amount."
    >
      <AccountEntryField
        label="Source Account"
        options={accounts}
        value={account}
        setValue={
          setAccount === null
            ? null
            : (nextValue): void => {
                setAccount(
                  accounts.find(
                    (candidate) => candidate.id === nextValue?.id,
                  ) ?? null,
                );
              }
        }
        filter={accountFilter}
      />
      <StringEntryField
        label="Source Location"
        value={location}
        setValue={account === null ? setLocation : null}
      />
      {renderLinesAndDeductions(
        "Income Lines",
        "Add the gross income amounts that make up this transaction.",
        incomeLines,
        setIncomeLines,
        "Add Income Line",
        false,
        createEmptyLine,
      )}
      {renderLinesAndDeductions(
        "Income Deductions",
        "Add optional deductions withheld before the income is deposited.",
        incomeDeductions,
        setIncomeDeductions,
        "Add Deduction",
        true,
        createEmptyDeduction,
      )}
    </TransactionFrame>
  );
};

export type { IncomeLineDraft, IncomeDeductionDraft };
export default IncomeTransactionSourceFrame;
