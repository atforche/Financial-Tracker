import { Box, Stack, Typography } from "@mui/material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * A described amount entered in the expected income editor.
 */
interface AmountItem {
  readonly description: string;
  readonly amount: number;
}

/**
 * Props for the ExpectedIncomeAmountEditor component.
 */
interface ExpectedIncomeAmountEditorProps {
  readonly title: string;
  readonly description: string;
  readonly addLabel: string;
  readonly items: AmountItem[];
  readonly setItems: (items: AmountItem[]) => void;
  readonly requireItem?: boolean;
}

/**
 * Edits an expected-income collection whose items contain a description and amount.
 */
const ExpectedIncomeAmountEditor = function ({
  title,
  description,
  addLabel,
  items,
  setItems,
  requireItem = false,
}: ExpectedIncomeAmountEditorProps): JSX.Element {
  const updateItem = function (
    index: number,
    changes: Partial<AmountItem>,
  ): void {
    setItems(
      items.map((current, itemIndex) =>
        itemIndex === index ? { ...current, ...changes } : current,
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
      <CollectionEditor
        items={items}
        label={title}
        setItems={setItems}
        createItem={() => ({ description: "", amount: 0 })}
        addLabel={addLabel}
        canDeleteItem={(_, __, current) => !requireItem || current.length > 1}
        itemContainerSx={{
          display: "grid",
          gap: 1.5,
          gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))" },
        }}
        renderItem={(item, index, controls) => (
          <InsetFrame
            key={index}
            sx={{
              display: "grid",
              gap: 1.5,
              // On small screens the delete button sits beside the description, with the amount below
              gridTemplateColumns: {
                xs: "minmax(0, 1fr) auto",
                md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
              },
              gridTemplateAreas: {
                xs: '"description delete" "amount amount"',
                md: '"description amount delete"',
              },
              alignItems: "start",
            }}
          >
            <Box sx={{ gridArea: "description" }}>
              <StringEntryField
                label="Description"
                value={item.description}
                autoFocus={controls.autoFocus}
                setValue={(newDescription): void => {
                  updateItem(index, { description: newDescription });
                }}
              />
            </Box>
            <Box sx={{ gridArea: "amount", "& > *": { width: "100%" } }}>
              <CurrencyEntryField
                label="Amount"
                value={item.amount}
                setValue={(amount): void => {
                  updateItem(index, { amount: amount ?? 0 });
                }}
              />
            </Box>
            <Box
              sx={{
                gridArea: "delete",
                display: "flex",
                justifyContent: "center",
                pt: 1.25,
              }}
            >
              {controls.deleteButton}
            </Box>
          </InsetFrame>
        )}
      />
    </Stack>
  );
};

export default ExpectedIncomeAmountEditor;
