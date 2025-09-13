import type { Account, AccountKey } from "@data/Account";
import { AddCircleOutline, Delete, Info, ModeEdit } from "@mui/icons-material";
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import AccountDialog from "@ui/dataEntry/AccountDialog";
import Action from "@ui/framework/listframe/Action";
import ActionColumn from "@ui/framework/listframe/ActionColumn";
import BooleanColumn from "@ui/framework/listframe/BooleanColumn";
import type Column from "@ui/framework/listframe/Column";
import DialogMode from "@core/fieldValues/DialogMode";
import HeaderAction from "@ui/framework/listframe/HeaderAction";
import StringColumn from "@ui/framework/listframe/StringColumn";
import { getAllAccounts } from "@data/AccountRepository";
import { useState } from "react";

/**
 * Interface representing the state of the child dialog.
 * @param {boolean} isOpen - Boolean flag indicating whether the dialog is open.
 * @param {DialogMode} mode - Current mode for the dialog.
 * @param {AccountKey | null} key - Key of the current Account for the dialog, or null if no Account is selected.
 */
interface DialogState {
  isOpen: boolean;
  mode: DialogMode;
  key: AccountKey | null;
}

const rows: Account[] = getAllAccounts();

/**
 * Component that provides a list of accounts and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of accounts with various action buttons.
 */
const AccountEntryList = function (): JSX.Element {
  const [dialogState, setDialogState] = useState<DialogState>({
    isOpen: false,
    mode: DialogMode.Create,
    key: null,
  });

  const columns: Column<Account>[] = [
    new StringColumn<Account>("Name", (value) => value.name),
    new StringColumn<Account>("Type", (value) => value.type.toString()),
    new BooleanColumn<Account>("Is Active", (value) => value.isActive),
    new ActionColumn<Account>(
      new HeaderAction(<AddCircleOutline />, "Add", () => {
        setDialogState({ isOpen: true, mode: DialogMode.Create, key: null });
      }),
      [
        new Action<Account>(<Info />, (row: Account) => () => {
          setDialogState({ isOpen: true, mode: DialogMode.View, key: row.key });
        }),
        new Action<Account>(<ModeEdit />, (row: Account) => () => {
          setDialogState({
            isOpen: true,
            mode: DialogMode.Update,
            key: row.key,
          });
        }),
        new Action<Account>(<Delete />, (row: Account) => () => {
          setDialogState({
            isOpen: true,
            mode: DialogMode.Delete,
            key: row.key,
          });
        }),
      ],
    ),
  ];

  return (
    <Box sx={{ paddingLeft: "25px" }}>
      <Typography
        variant="h6"
        sx={{ paddingBottom: "25px", paddingTop: "25px" }}
      >
        Accounts
      </Typography>
      <Paper
        sx={{
          [`& .MuiIconButton-root`]: {
            display: "none",
          },
          [`& .MuiTableRow-hover:hover`]: {
            [`.MuiIconButton-root`]: {
              display: "inline-block",
            },
          },
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                {columns.map((column) => column.getHeaderElement())}
              </TableRow>
            </TableHead>
            <TableBody>
              {rows
                .sort((first, second) => Number(first.key - second.key))
                .map((row) => (
                  <TableRow hover role="checkbox" tabIndex={-1} key={row.name}>
                    {columns.map((column) => column.getRowElement(row))}
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
      <AccountDialog
        isOpen={dialogState.isOpen}
        mode={dialogState.mode}
        onClose={() => {
          setDialogState({
            isOpen: false,
            mode: dialogState.mode,
            key: dialogState.key,
          });
        }}
        accountKey={dialogState.key}
      />
    </Box>
  );
};

export default AccountEntryList;
