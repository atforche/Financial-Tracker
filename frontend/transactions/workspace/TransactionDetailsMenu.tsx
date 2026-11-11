"use client";

import { Divider, IconButton, Menu, MenuItem } from "@mui/material";
import { type JSX, useState } from "react";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import Link from "next/link";
import MoreVert from "@mui/icons-material/MoreVert";
import type { Transaction } from "@/transactions/types";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";

/**
 * Props for the TransactionDetailsMenu component.
 */
interface TransactionDetailsMenuProps {
  readonly transaction: Transaction;
  readonly editUrl: string;
  readonly postedAccountCount: number;
  readonly currentUrl: string;
  readonly deleteRedirectUrl: string;
}

/**
 * Collects a transaction's actions in a menu: editing it, then unposting it from every posted account when more than one
 * is posted, or deleting it when nothing is.
 */
const TransactionDetailsMenu = function ({
  transaction,
  editUrl,
  postedAccountCount,
  currentUrl,
  deleteRedirectUrl,
}: TransactionDetailsMenuProps): JSX.Element {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const closeThen = function (action: () => void): () => void {
    return (): void => {
      setAnchor(null);
      action();
    };
  };
  return (
    <>
      <IconButton
        aria-label="Transaction actions"
        onClick={(event) => {
          setAnchor(event.currentTarget);
        }}
      >
        <MoreVert />
      </IconButton>
      <Menu
        anchorEl={anchor}
        open={anchor !== null}
        onClose={() => {
          setAnchor(null);
        }}
        // Keeps the confirmation dialogs mounted after the menu closes
        keepMounted
      >
        <MenuItem component={Link} href={editUrl}>
          Edit
        </MenuItem>
        {/* With one posted account, that account's own Unpost button covers it */}
        {postedAccountCount === 1 ? null : <Divider />}
        {postedAccountCount > 1 ? (
          <UnpostTransactionForm
            transaction={transaction}
            redirectUrl={currentUrl}
            trigger={(openDialog) => (
              <MenuItem onClick={closeThen(openDialog)}>Unpost All</MenuItem>
            )}
          />
        ) : null}
        {postedAccountCount === 0 ? (
          <DeleteTransactionForm
            transaction={transaction}
            redirectUrl={deleteRedirectUrl}
            trigger={(openDialog) => (
              <MenuItem
                sx={{ color: "error.main" }}
                onClick={closeThen(openDialog)}
              >
                Delete
              </MenuItem>
            )}
          />
        ) : null}
      </Menu>
    </>
  );
};

export default TransactionDetailsMenu;
