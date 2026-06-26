import { AddCircleOutline, DeleteOutline } from "@mui/icons-material";
import { Box, Button, IconButton, Stack, Typography } from "@mui/material";
import type {
  IncomeDeduction,
  IncomeLine,
} from "@/transactions/incomeTransaction";
import type {
  IncomeDeductionDraft,
  IncomeLineDraft,
} from "@/transactions/workspace/income/helpers";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Type representing an item under the source of an income transaction.
 */
type IncomeTransactionSourceItem =
  | IncomeLine
  | IncomeDeduction
  | IncomeLineDraft
  | IncomeDeductionDraft;

/**
 * Props for the IncomeTransactionSourceItemFrame component.
 */
interface IncomeTransactionSourceItemFrameProps<
  T extends IncomeTransactionSourceItem,
> {
  readonly title: string;
  readonly description: string;
  readonly items: T[];
  readonly setItems?: ((items: T[]) => void) | null;
  readonly createEmptyItem?: (() => T) | null;
  readonly addLabel?: string | null;
  readonly allowEmpty?: boolean;
}

/**
 * Displays an individual item under an income transaction source.
 */
const IncomeTransactionSourceItemFrame = function <
  T extends IncomeTransactionSourceItem,
>({
  title,
  description,
  items,
  setItems = null,
  createEmptyItem = null,
  addLabel = null,
  allowEmpty = false,
}: IncomeTransactionSourceItemFrameProps<T>): JSX.Element {
  const editable =
    setItems !== null && createEmptyItem !== null && addLabel !== null;
  const updateItem = function (index: number, recipe: (current: T) => T): void {
    if (!editable) {
      return;
    }
    setItems(
      items.map((item, itemIndex) =>
        itemIndex === index ? recipe(item) : item,
      ),
    );
  };

  return (
    <Stack spacing={1.5}>
      <Stack spacing={0.25}>
        <Typography variant="subtitle2">{title}</Typography>
        <Typography variant="body2" color="text.secondary">
          {description}
        </Typography>
      </Stack>
      {items.length === 0 && !editable ? (
        <Box
          sx={{
            border: "1px dashed",
            borderColor: "divider",
            borderRadius: 3,
            p: 2,
            textAlign: "center",
          }}
        >
          <Typography variant="body2" color="text.secondary">
            No items available.
          </Typography>
        </Box>
      ) : (
        items.map((item, index) => (
          <Box
            key={`${title}-${index}`}
            sx={{
              display: "grid",
              gap: 1.5,
              gridTemplateColumns: {
                xs: "1fr",
                md: editable
                  ? "minmax(0, 1.8fr) minmax(180px, 1fr) auto"
                  : "minmax(0, 1.8fr) minmax(180px, 1fr)",
              },
              alignItems: "start",
            }}
          >
            <StringEntryField
              label="Description"
              value={item.description}
              setValue={
                editable
                  ? (nextDescription): void => {
                      updateItem(index, (current) => ({
                        ...current,
                        description: nextDescription,
                      }));
                    }
                  : null
              }
            />
            <CurrencyEntryField
              label="Amount"
              value={item.amount}
              setValue={
                editable
                  ? (nextAmount): void => {
                      updateItem(index, (current) => ({
                        ...current,
                        amount: nextAmount,
                      }));
                    }
                  : null
              }
            />
            {editable ? (
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
            ) : null}
          </Box>
        ))
      )}
      {editable ? (
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
      ) : null}
    </Stack>
  );
};

export default IncomeTransactionSourceItemFrame;
