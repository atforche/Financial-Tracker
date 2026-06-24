import type { JSX, ReactNode } from "react";
import { Paper, Stack, Typography } from "@mui/material";

interface TransactionSectionProps {
  readonly title: string;
  readonly description: string;
  readonly children: ReactNode;
  readonly headerAction?: ReactNode;
}

/**
 * Renders a reusable content section inside a transaction form.
 */
const TransactionSection = function ({
  title,
  description,
  children,
  headerAction,
}: TransactionSectionProps): JSX.Element {
  return (
    <Paper
      variant="outlined"
      sx={{
        borderRadius: 4,
        p: { xs: 2.5, md: 3 },
      }}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "flex-start" }}
        >
          <Stack spacing={0.5}>
            <Typography variant="h6">{title}</Typography>
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          </Stack>
          {headerAction ?? null}
        </Stack>
        {children}
      </Stack>
    </Paper>
  );
};

export default TransactionSection;
