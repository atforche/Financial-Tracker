import { Box, Stack, Typography } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Represents an item in the expected-income source section, including its description and amount.
 */
interface ExpectedIncomeSourceItem {
  readonly description: string;
  readonly amount: number;
}

/**
 * Props for the ExpectedIncomeSourceItemSection component, which displays a section of expected-income source items.
 */
interface ExpectedIncomeSourceItemSectionProps {
  readonly title: string;
  readonly description: string;
  readonly items: readonly ExpectedIncomeSourceItem[];
}

/**
 * Displays expected-income source items in responsive, lightly framed groups.
 */
const ExpectedIncomeSourceItemSection = function ({
  title,
  description,
  items,
}: ExpectedIncomeSourceItemSectionProps): JSX.Element {
  return (
    <Stack spacing={1.5}>
      <Stack spacing={0.25}>
        <Typography variant="subtitle2">{title}</Typography>
        <Typography variant="body2" color="text.secondary">
          {description}
        </Typography>
      </Stack>
      {items.length === 0 ? (
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
        <Box
          sx={{
            display: "grid",
            gap: 1.5,
            gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))" },
          }}
        >
          {items.map((item, index) => (
            <InsetFrame
              key={`${title}-${index}`}
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: "minmax(0, 1.8fr) minmax(180px, 1fr)",
                alignItems: "start",
              }}
            >
              <StringEntryField label="Description" value={item.description} />
              <CurrencyEntryField label="Amount" value={item.amount} />
            </InsetFrame>
          ))}
        </Box>
      )}
    </Stack>
  );
};

export default ExpectedIncomeSourceItemSection;
