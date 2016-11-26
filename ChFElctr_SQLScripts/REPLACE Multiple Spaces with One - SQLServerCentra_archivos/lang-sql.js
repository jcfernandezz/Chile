// Copyright (C) 2008 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.



/**
 * @fileoverview
 * Registers a language handler for SQL.
 *
 *
 * To use, include prettify.js and this file in your HTML page.
 * Then put your code in an HTML tag like
 *      <pre class="prettyprint lang-sql">(my SQL code)</pre>
 *
 *
 * http://savage.net.au/SQL/sql-99.bnf.html is the basis for the grammar, and
 * http://msdn.microsoft.com/en-us/library/aa238507(SQL.80).aspx as the basis
 * for the keyword list.
 *
 * @author mikesamuel@gmail.com
 */

PR.registerLangHandler(
    PR.createSimpleLexer(
        [
         // Whitespace
         [PR.PR_PLAIN,       /^[\t\n\r \xA0]+/, null, '\t\n\r \xA0'],
         // A double or single quoted, possibly multi-line, string.
         [PR.PR_STRING,      /^(?:"(?:[^\"\\]|\\.)*"|'(?:[^\'\\]|\\.)*')/, null,
          '"\'']
        ],
        [
         // A comment is either a line comment that starts with two dashes, or
         // two dashes preceding a long bracketed block.
         [PR.PR_COMMENT, /^(?:--[^\r\n]*|\/\*[\s\S]*?(?:\*\/|$))/],
         // Blue
         [PR.PR_KEYWORD, /^(?:ADD|ALTER|AS|ASC|AUTHORIZATION|BACKUP|BEGIN|BREAK|BROWSE|BULK|BY|CASCADE|CASE|CHECK|CHECKPOINT|CLOSE|CLUSTERED|COLLATE|COLUMN|COMMIT|COMPUTE|CONSTRAINT|CONTAINS|CONTAINSTABLE|CONTINUE|CREATE|CURRENT|CURRENT_DATE|CURRENT_TIME|CURSOR|DATABASE|DBCC|DEALLOCATE|DECLARE|DEFAULT|DELETE|DENY|DESC|DISK|DISTINCT|DISTRIBUTED|DOUBLE|DROP|DUMP|ELSE|END|ERRLVL|ESCAPE|EXCEPT|EXEC|EXECUTE|EXIT|FETCH|FILE|FILLFACTOR|FOR|FOREIGN|FREETEXT|FREETEXTTABLE|FROM|FULL|FUNCTION|GOTO|GRANT|GROUP|HAVING|HOLDLOCK|IDENTITY|IDENTITYCOL|IDENTITY_INSERT|IF|INDEX|INSERT|INTERSECT|INTO|KEY|KILL|LINENO|LOAD|NATIONAL|NOCHECK|NONCLUSTERED|OF|OFF|OFFSETS|ON|OPEN|OPENDATASOURCE|OPENQUERY|OPENROWSET|OPENXML|OPTION|ORDER|OVER|PARTITION|PERCENT|PLAN|PRECISION|PRIMARY|PRINT|PROC|PROCEDURE|PUBLIC|RAISERROR|READ|READTEXT|RECONFIGURE|REFERENCES|REPLICATION|RESTORE|RESTRICT|RETURN|RETURNS|REVOKE|ROLLBACK|ROWCOUNT|ROWGUIDCOL|RULE|SAVE|SCHEMA|SELECT|SET|SETUSER|SHUTDOWN|STATISTICS|TABLE|TEXTSIZE|THEN|TO|TOP|TRAN|TRANSACTION|TRIGGER|TRUNCATE|TSEQUAL|UNION|UNIQUE|UPDATE|UPDATETEXT|USE|VALUES|VARYING|VIEW|WAITFOR|WHEN|WHERE|WHILE|WITH|WRITETEXT)(?=[^\w-]|$)/i, null],
         // Pink
         ['kwd2', /^(?:ABS|ACOS|ASCII|ASIN|ATAN|ATN2|AVG|CEILING|CHARINDEX|CHECKSUM_AGG|COALESCE|CONVERT|COS|COT|COUNT|COUNT_BIG|CURRENT_TIMESTAMP|CURRENT_USER|DATEADD|DATEDIFF|DATENAME|DATEPART|DAY|DEGREES|DIFFERENCE|EXP|FLOOR|GROUPING|ISDATE|LEN|LOG|LOG10|LOWER|LTRIM|MAX|MIN|MONTH|NULLIF|PATINDEX|PI|POWER|QUOTENAME|RADIANS|RAND|REPLACE|REPLICATE|REVERSE|ROUND|ROW_NUMBER|RTRIM|SESSION_USER|SIGN|SIN|SOUNDEX|SPACE|SQRT|SQUARE|STDEV|STDEVP|STR|STUFF|SUBSTRING|SUM|SWITCHOFFSET|SYSTEM_USER|TAN|TODATETIMEOFFSET|UNICODE|UPPER|USER|VAR|VARP|YEAR)(?=[^\w-]|$)/i, null],
         // Grey
         ['kwd3', /^(?:ALL|AND|ANY|APPLY|BETWEEN|CROSS|EXISTS|IN|INNER|IS|JOIN|LEFT|LIKE|NOT|NULL|OR|OUTER|RIGHT|SOME)(?=[^\w-]|$)/i, null],
         [PR.PR_TYPE, /^(?:BIT|INT|SMALLINT|TINYINT|DECIMAL|NUMERIC|MONEY|SMALLMONEY|FLOAT|REAL|DATETIME|SMALLDATETIME|CURSOR|TIMESTAMP|UNIQUEIDENTIFIER|CHAR|VARCHAR|TEXT|NCHAR|NVARCHAR|NTEXT|BINARY|VARBINARY|IMAGE)(?=[^\w-]|$|\()/i, null],
         // A number is a hex integer literal, a decimal real literal, or in
         // scientific notation.
         [PR.PR_LITERAL,
          /^[+-]?(?:0x[\da-f]+|(?:(?:\.\d+|\d+(?:\.\d*)?)(?:e[+\-]?\d+)?))/i],
         // An identifier
         [PR.PR_PLAIN, /^[a-z_][\w-]*/i],
         // A run of punctuation (@ and # are not punctuation e.g. #table, @param)
         [PR.PR_PUNCTUATION, /^[^\w\t\n\r @#\*\'\xA0]+/]
        ]),
    ['sql']);
