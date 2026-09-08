import { Box, Stack, Typography } from "@mui/material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";

interface AmountItem {
  readonly description: string;
  readonly amount: number;
}

interface Props {
  readonly title: string;
  readonly description: string;
  readonly addLabel: string;
  readonly items: AmountItem[];
  readonly setItems?: ((items: AmountItem[]) => void) | null;
  readonly requireItem?: boolean;
}

/** Displays an expected-income collection whose items contain a description and amount. */
const ExpectedIncomeAmountEditor = function ({
  title,
  description,
  addLabel,
  items,
  setItems = null,
  requireItem = false,
}: Props): JSX.Element {
  const readOnly = setItems === null;
  return (
    <Stack spacing={1.5}>
      {!readOnly || items.length <= 1 ? (
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">{title}</Typography>
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        </Stack>
      ) : null}
      {readOnly && items.length === 0 ? (
        <Typography color="text.secondary">No items available.</Typography>
      ) : null}
      <CollectionEditor
        items={items}
        label={title}
        readOnly={readOnly}
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
              gridTemplateColumns: {
                xs: "1fr",
                md: readOnly
                  ? "minmax(0, 1.8fr) minmax(180px, 1fr)"
                  : "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
              },
              alignItems: "start",
            }}
          >
            <StringEntryField
              label="Description"
              value={item.description}
              autoFocus={controls.autoFocus}
              setValue={
                readOnly
                  ? null
                  : (newDescription): void => {
                      setItems(
                        items.map((current, itemIndex) =>
                          itemIndex === index
                            ? { ...current, description: newDescription }
                            : current,
                        ),
                      );
                    }
              }
            />
            <CurrencyEntryField
              label="Amount"
              value={item.amount}
              setValue={
                readOnly
                  ? null
                  : (amount): void => {
                      setItems(
                        items.map((current, itemIndex) =>
                          itemIndex === index
                            ? { ...current, amount: amount ?? 0 }
                            : current,
                        ),
                      );
                    }
              }
            />
            {readOnly ? null : (
              <Box
                sx={{
                  display: "flex",
                  justifyContent: { xs: "flex-end", md: "center" },
                  pt: { xs: 0, md: 1.25 },
                }}
              >
                {controls.deleteButton}
              </Box>
            )}
          </InsetFrame>
        )}
      />
    </Stack>
  );
};

export default ExpectedIncomeAmountEditor;
