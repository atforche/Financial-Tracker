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
import type { Fund, FundKey } from "@data/Fund";
import Action from "@ui/framework/listframe/Action";
import ActionColumn from "@ui/framework/listframe/ActionColumn";
import type Column from "@ui/framework/listframe/Column";
import DialogMode from "@core/fieldValues/DialogMode";
import FundDialog from "@ui/dataEntry/FundDialog";
import HeaderAction from "@ui/framework/listframe/HeaderAction";
import StringColumn from "@ui/framework/listframe/StringColumn";
import { getAllFunds } from "@data/FundRepository";
import { useQuery } from "@data/useQuery";
import { useState } from "react";

/**
 * Interface representing the state of the child dialog.
 * @param {boolean} isOpen - Boolean flag indicating whether the dialog is open.
 * @param {DialogMode} mode - Current mode for the dialog.
 * @param {FundKey | null} key - Key of the current Fund for the dialog, or null if no Fund is selected.
 */
interface DialogState {
  isOpen: boolean;
  mode: DialogMode;
  key: FundKey | null;
}

/**
 * Component that provides a list of funds and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of funds with various action buttons.
 */
const FundEntryList = function (): JSX.Element {
  const [dialogState, setDialogState] = useState<DialogState>({
    isOpen: false,
    mode: DialogMode.Create,
    key: null,
  });

  const { data, refetch } = useQuery<Fund[]>({
    queryFunction: getAllFunds,
    initialData: [],
  });

  const columns: Column<Fund>[] = [
    new StringColumn<Fund>("Name", (value) => value.name),
    new StringColumn<Fund>("Description", (value) => value.description ?? ""),
    new ActionColumn<Fund>(
      new HeaderAction(<AddCircleOutline />, "Add", () => {
        setDialogState({ isOpen: true, mode: DialogMode.Create, key: null });
      }),
      [
        new Action<Fund>(<Info />, (row: Fund) => () => {
          setDialogState({ isOpen: true, mode: DialogMode.View, key: row.key });
        }),
        new Action<Fund>(<ModeEdit />, (row: Fund) => () => {
          setDialogState({
            isOpen: true,
            mode: DialogMode.Update,
            key: row.key,
          });
        }),
        new Action<Fund>(<Delete />, (row: Fund) => () => {
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
        Funds
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
              {data
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
      <FundDialog
        isOpen={dialogState.isOpen}
        mode={dialogState.mode}
        onClose={() => {
          setDialogState({
            isOpen: false,
            mode: dialogState.mode,
            key: dialogState.key,
          });
          refetch();
        }}
        fundKey={dialogState.key}
      />
    </Box>
  );
};

export default FundEntryList;
