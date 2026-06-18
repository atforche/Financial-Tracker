import type { Account, AccountIdentifier } from "@/accounts/types";
import { Box, Stack, Typography } from "@mui/material";
import AccountEntryField from "@/accounts/AccountEntryField";
import ArrowForward from "@mui/icons-material/ArrowForward";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionSection from "@/transactions/workspace/TransactionSection";

interface TransactionAccountPairSectionProps {
  readonly title: string;
  readonly description: string;
  readonly accounts: Account[];
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftAccount: Account | null;
  readonly rightAccount: Account | null;
  readonly setLeftAccount: ((account: Account | null) => void) | null;
  readonly setRightAccount: ((account: Account | null) => void) | null;
  readonly leftFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly rightFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly leftLocationLabel?: string | null;
  readonly leftLocationValue?: string;
  readonly setLeftLocationValue?: ((location: string) => void) | null;
  readonly rightLocationLabel?: string | null;
  readonly rightLocationValue?: string;
  readonly setRightLocationValue?: ((location: string) => void) | null;
}

/**
 * Displays a pair of account selectors for transaction source and destination fields.
 */
const TransactionAccountPairSection = function ({
  title,
  description,
  accounts,
  leftLabel,
  rightLabel,
  leftAccount,
  rightAccount,
  setLeftAccount,
  setRightAccount,
  leftFilter = null,
  rightFilter = null,
  leftLocationLabel = null,
  leftLocationValue = "",
  setLeftLocationValue = null,
  rightLocationLabel = null,
  rightLocationValue = "",
  setRightLocationValue = null,
}: TransactionAccountPairSectionProps): JSX.Element {
  const leftHasLocationOption =
    leftLocationLabel !== null && setLeftLocationValue !== null;
  const rightHasLocationOption =
    rightLocationLabel !== null && setRightLocationValue !== null;

  const renderAccountField = function (
    label: string,
    value: Account | null,
    setValue: ((account: Account | null) => void) | null,
    filter: ((account: AccountIdentifier) => boolean) | null,
  ): JSX.Element {
    return (
      <AccountEntryField
        label={label}
        options={accounts}
        value={value}
        setValue={
          setValue === null
            ? null
            : (newValue): void => {
                setValue(
                  accounts.find((account) => account.id === newValue?.id) ??
                    null,
                );
              }
        }
        filter={filter}
      />
    );
  };

  const renderSide = function (
    accountField: JSX.Element,
    locationLabel: string | null,
    locationValue: string,
    setLocationValue: ((location: string) => void) | null,
    showLocationOption: boolean,
  ): JSX.Element {
    return (
      <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
        <Stack spacing={2} sx={{ width: "100%" }}>
          {accountField}
          {showLocationOption && locationLabel !== null ? (
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
              <StringEntryField
                label={locationLabel}
                value={locationValue}
                setValue={setLocationValue}
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
          renderAccountField(
            leftLabel,
            leftAccount,
            setLeftAccount,
            leftFilter,
          ),
          leftLocationLabel,
          leftLocationValue,
          setLeftLocationValue,
          leftHasLocationOption,
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
          renderAccountField(
            rightLabel,
            rightAccount,
            setRightAccount,
            rightFilter,
          ),
          rightLocationLabel,
          rightLocationValue,
          setRightLocationValue,
          rightHasLocationOption,
        )}
      </Box>
    </TransactionSection>
  );
};

export default TransactionAccountPairSection;
