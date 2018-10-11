grammar ADL;


statement : archelement | assertion;
archelement : (component | connector | system ) '{' feature (feature*)?  '}' NEWLINE;
feature : ((port | role ) '('paramdefs ')' ASG sequence NEWLINE)   
        | glue 
		| define 
		| attach
		| link;

assertion: 'assert' ID verification NEWLINE;
verification: (DEADLOCKFREE | LTL);

system : 'system' ID;
component : 'component' ID;
connector : 'connector' ID;

port : 'port' ID;
role : 'role' ID;

glue : 'glue' process (processexpr*)?;
processexpr :  INTERLEAVE process;

define : 'define' ID ASG ID;
attach : 'attach' process ASG process (processexpr*)?;
link : 'link' process ASG process;
//attach : 'attach' ID '.' process ASG ID '.' process;


channel : ID (channelInput|channelOutput);
channelInput : '?' ID;
channelOutput : '!' ID;
process : (ID '.')?ID ('('paramdefs ')')?;
event :  (process|channel) TRANSIT;
sequence :  event event*;

paramdefs:  (ID COMMA)* ID ;

NEWLINE  : ('\r'? '\n' | '\r')+ ;
ID : ([a-zA-Z_] | [0-9_])* ;
WHITESPACE : (' ' | '\t')+ -> skip;
SEMICOLON : ';';
COMMA : ',';
ASG : '=';
TRANSIT : '->';
INTERLEAVE : '|||';
CHOICE : '[]';
DEADLOCKFREE : 'deadlockfree';
LTL : 'LTL';