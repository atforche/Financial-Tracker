import { type Account, AccountType } from "../../data/Account";
import { AddCircleOutline, Delete, Info, ModeEdit } from "@mui/icons-material";
import {
  Box,
  Button,
  Checkbox,
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
import { getAccounts } from "../../data/AccountRepository";

interface Column {
  id: "name" | "type" | "active" | "actions";
  label: string | JSX.Element;
  mapping: (value: Account) => string | JSX.Element;
  align?: "center";
  width?: number;
}

const columns: Column[] = [
  { id: "name", label: "Name", mapping: (value) => value.name },
  { id: "type", label: "Type", mapping: (value) => AccountType[value.type] },
  {
    id: "active",
    label: "Is Active",
    mapping: (value) =>
      value.isActive ? <Checkbox disabled checked /> : <Checkbox disabled />,
    width: 100,
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
        >
          Add
        </Button>
      </Box>
    ),
    mapping: () => (
      <>
        <IconButton>
          <Info />
        </IconButton>
        <IconButton>
          <ModeEdit />
        </IconButton>
        <IconButton>
          <Delete />
        </IconButton>
      </>
    ),
    width: 125,
  },
];

const rows: Account[] = getAccounts();

/**
 * Component that provides a list of accounts and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of accounts with various action buttons.
 */
const AccountEntryList = function (): JSX.Element {
  const defaultColumnWidth = 170;
  const defaultColumnAlign = "left";

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
              {rows.map((row) => (
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
    </Box>
  );
};

export default AccountEntryList;
