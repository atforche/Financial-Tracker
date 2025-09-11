import { AddCircleOutline, Delete, Info, ModeEdit } from "@mui/icons-material";
import {
  Box,
  Button,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import type { Fund, FundKey } from "@data/Fund";
import DialogMode from "@core/fieldValues/DialogMode";
import FundDialog from "./FundDialog";
import { getAllFunds } from "@data/FundRepository";
import { useQuery } from "@data/useQuery";
import { useState } from "react";

/**
 * Interface representing a column in the EntryList.
 * @param {string} id - Id that uniquely identifies this Column.
 * @param {string} label - Label for this Column.
 * @param {Function} mapping - Mapping function that maps an Account to the value that should be displayed in this column.
 * @param {string} align - Alignment for this column. Defaults to "left" if not provided.
 * @param {number} width - Width for this column. Defaults to 170 if not provided.
 */
interface Column {
  id: "name" | "description" | "actions";
  label: string | JSX.Element;
  mapping: (value: Fund) => string | JSX.Element;
  align?: "center";
  width?: number;
}

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
  const defaultColumnWidth = 170;
  const defaultColumnAlign = "left";

  const [dialogState, setDialogState] = useState<DialogState>({
    isOpen: false,
    mode: DialogMode.Create,
    key: null,
  });

  const { data, refetch } = useQuery<Fund[]>({
    queryFunction: getAllFunds,
    initialData: [],
  });

  const columns: Column[] = [
    { id: "name", label: "Name", mapping: (value) => value.name },
    {
      id: "description",
      label: "Description",
      mapping: (value) => value.description ?? "",
    },
    {
      align: "center",
      id: "actions",
      label: (
        <Box sx={{ verticalAlign: "middle" }}>
          <Button
            variant="contained"
            startIcon={<AddCircleOutline />}
            disableElevation
            sx={{ backgroundColor: "primary", border: 1, borderColor: "white" }}
            onClick={() => {
              setDialogState({
                isOpen: true,
                mode: DialogMode.Create,
                key: null,
              });
            }}
          >
            Add
          </Button>
        </Box>
      ),
      mapping: (row: Fund) => (
        <>
          <IconButton
            onClick={() => {
              setDialogState({
                isOpen: true,
                mode: DialogMode.View,
                key: row.key,
              });
            }}
          >
            <Info />
          </IconButton>
          <IconButton
            onClick={() => {
              setDialogState({
                isOpen: true,
                mode: DialogMode.Update,
                key: row.key,
              });
            }}
          >
            <ModeEdit />
          </IconButton>
          <IconButton>
            <Delete
              onClick={() => {
                setDialogState({
                  isOpen: true,
                  mode: DialogMode.Delete,
                  key: row.key,
                });
              }}
            />
          </IconButton>
        </>
      ),
      width: 125,
    },
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
                {columns.map((column) => (
                  <TableCell
                    key={column.id}
                    align={column.align ?? defaultColumnAlign}
                    style={{ minWidth: column.width ?? defaultColumnWidth }}
                    sx={{ backgroundColor: "primary.main", color: "white" }}
                  >
                    {column.label}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {data
                .sort((first, second) => Number(first.key - second.key))
                .map((row) => (
                  <TableRow hover role="checkbox" tabIndex={-1} key={row.name}>
                    {columns.map((column) => (
                      <TableCell
                        key={column.id}
                        align={column.align ?? defaultColumnAlign}
                      >
                        {column.mapping(row)}
                      </TableCell>
                    ))}
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
