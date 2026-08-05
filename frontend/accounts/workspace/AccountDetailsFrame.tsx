"use client";

import type { Dispatch, JSX, ReactNode, SetStateAction } from "react";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { AccountType } from "@/accounts/types";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import CreatableComboBoxEntryField from "@/framework/forms/CreatableComboBoxEntryField";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the AccountDetailsFrame component.
 */
interface AccountDetailsFrameProps {
  readonly color?: FrameColor;
  readonly name: string;
  readonly setName?: Dispatch<SetStateAction<string>> | null;
  readonly nameErrorMessage?: string | null;
  readonly financialInstitution: string | null;
  readonly financialInstitutions: readonly string[];
  readonly setFinancialInstitution?: ((newValue: string | null) => void) | null;
  readonly financialInstitutionErrorMessage?: string | null;
  readonly accountType: AccountType | null;
  readonly setAccountType?: ((newValue: AccountType | null) => void) | null;
  readonly accountTypeErrorMessage?: string | null;
  readonly headerContent?: ReactNode;
}

/**
 * Displays the shared account name and type section used across account flows.
 */
const AccountDetailsFrame = function ({
  color = "info",
  name,
  setName = null,
  nameErrorMessage = null,
  financialInstitution,
  financialInstitutions,
  setFinancialInstitution = null,
  financialInstitutionErrorMessage = null,
  accountType,
  setAccountType = null,
  accountTypeErrorMessage = null,
  headerContent = null,
}: AccountDetailsFrameProps): JSX.Element {
  return (
    <Frame title="Details" color={color} headerContent={headerContent}>
      <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={nameErrorMessage}
        />
        <CreatableComboBoxEntryField
          label="Financial Institution"
          options={financialInstitutions}
          value={financialInstitution}
          setValue={setFinancialInstitution}
          errorMessage={financialInstitutionErrorMessage}
        />
        <AccountTypeEntryField
          label="Type"
          value={accountType}
          setValue={setAccountType}
          errorMessage={accountTypeErrorMessage}
        />
      </ResponsiveGrid>
    </Frame>
  );
};

export default AccountDetailsFrame;
