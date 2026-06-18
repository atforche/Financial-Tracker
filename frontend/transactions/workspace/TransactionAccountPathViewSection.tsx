import { Box, Stack, Typography } from "@mui/material";
import ArrowForward from "@mui/icons-material/ArrowForward";
import type { JSX } from "react";
import type { TransactionAccount } from "@/transactions/types";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";

interface TransactionAccountPathViewSectionProps {
  readonly title: string;
  readonly description: string;
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftAccount: TransactionAccount | null;
  readonly rightAccount: TransactionAccount | null;
  readonly leftLocationLabel?: string | null;
  readonly leftLocationValue?: string | null;
  readonly rightLocationLabel?: string | null;
  readonly rightLocationValue?: string | null;
}

/**
 * Displays the read-only account path for a transaction.
 */
const TransactionAccountPathViewSection = function ({
  title,
  description,
  leftLabel,
  rightLabel,
  leftAccount,
  rightAccount,
  leftLocationLabel = null,
  leftLocationValue = null,
  rightLocationLabel = null,
  rightLocationValue = null,
}: TransactionAccountPathViewSectionProps): JSX.Element {
  const leftHasLocationValue =
    leftLocationLabel !== null &&
    leftLocationValue !== null &&
    leftLocationValue !== "";
  const rightHasLocationValue =
    rightLocationLabel !== null &&
    rightLocationValue !== null &&
    rightLocationValue !== "";

  const renderAccountField = function (
    label: string,
    account: TransactionAccount | null,
  ): JSX.Element {
    return (
      <TransactionDisplayField
        label={label}
        value={account?.accountName ?? "None"}
        helperText={
          account !== null ? (
            <TransactionBalanceDetails
              previousPostedBalance={formatCurrency(
                account.previousAccountBalance.postedBalance,
              )}
              newPostedBalance={formatCurrency(
                account.newAccountBalance.postedBalance,
              )}
            />
          ) : null
        }
      />
    );
  };

  const renderSide = function (
    accountField: JSX.Element,
    locationLabel: string | null,
    locationValue: string | null,
    showLocationValue: boolean,
  ): JSX.Element {
    return (
      <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
        <Stack spacing={2} sx={{ width: "100%" }}>
          {accountField}
          {showLocationValue && locationLabel !== null ? (
            <>
              <Typography
                variant="body2"
                color="text.secondary"
                textAlign="center"
                textTransform="uppercase"
                letterSpacing="0.08em"
              >
                or
              </Typography>
              <TransactionDisplayField
                label={locationLabel}
                value={locationValue ?? ""}
              />
            </>
          ) : null}
        </Stack>
      </Box>
    );
  };

  return (
    <TransactionSection title={title} description={description}>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          alignItems: "stretch",
          gridTemplateColumns: {
            xs: "1fr",
            md: "minmax(0, 1fr) auto minmax(0, 1fr)",
          },
        }}
      >
        {renderSide(
          renderAccountField(leftLabel, leftAccount),
          leftLocationLabel,
          leftLocationValue,
          leftHasLocationValue,
        )}
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            minHeight: { xs: 0, md: 56 },
          }}
        >
          <Typography
            component="span"
            sx={{
              color: "text.secondary",
              fontSize: { xs: "2rem", md: "2.5rem" },
              lineHeight: 1,
            }}
          >
            <ArrowForward />
          </Typography>
        </Box>
        {renderSide(
          renderAccountField(rightLabel, rightAccount),
          rightLocationLabel,
          rightLocationValue,
          rightHasLocationValue,
        )}
      </Box>
    </TransactionSection>
  );
};

export default TransactionAccountPathViewSection;
