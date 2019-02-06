grammar ADL;


statement : archelement | assertion | LINE_COMMENT;
archelement : (component | connector | system ) '{' NEWLINE? feature (feature*)?  '}' NEWLINE;
feature : ((port | role ) '('paramdefs? ')' (ASG sequence)? SEMICOLON NEWLINE? )   
        | execute 
		| declare 
		| attach;

assertion: 'assert' verification SEMICOLON NEWLINE;
verification: ID ( DEADLOCKFREE | CIRCULARFREE | BOTTLENECKFREE | AMBIGUOUSINTERFACEFREE | LAVAFLOWFREE | DECOMPOSITIONFREE | POLTERGEISTSFREE | reachexpr  | ltlexpr );
ltlexpr: '|=' (( '(' | ')' | '[]' | '<>' | '!' | '?' | '&&' | '||' | '->' | '<->' | '/\\' | '\\/' | '.' | ID ))*;
reachexpr: REACHES ID;

system : 'system' ID;
component : 'component' ID;
connector : 'connector' ID;

port : 'port' ID;
role : 'role' ID;

execute : 'execute' process (processexpr*)?;
processexpr :  (INTERLEAVE | EMBED | CHOICE | PARALLEL) process;

declare : 'declare' ID ASG ID SEMICOLON NEWLINE?;
attach : 'attach' process ASG process (processexpr*)? SEMICOLON NEWLINE?;
//attach : 'attach' ID '.' process ASG ID '.' process;


channel : ID (channelInput|channelOutput);
channelInput : '?' ID;
channelOutput : '!' ID;
process : (ID '.')?ID ('('paramdefs? ')')?;
event :  (process|channel) ;
sequence :  event ((TRANSIT event)*)?;

paramdefs:  ((ID COMMA)*)? ID ;

NEWLINE  : ('\r'? '\n' | '\r')+ ;
DEADLOCKFREE : 'deadlockfree';
CIRCULARFREE : 'circularfree';
BOTTLENECKFREE : 'bottleneckfree';
AMBIGUOUSINTERFACEFREE : 'ambiguousinterfacefree';
LAVAFLOWFREE: 'lavaflowfree';
DECOMPOSITIONFREE: 'decompositionfree';
POLTERGEISTSFREE: 'poltergeistfree';
REACHES : 'reaches';
LTL : 'LTL';
ID : ([a-zA-Z_] | [0-9_])* ;

WHITESPACE : (' ' | '\t')+ -> skip;
SEMICOLON : ';';
COMMA : ',';
ASG : '=';
TRANSIT : '->';
INTERLEAVE : '|||';
PARALLEL : '||';
EMBED : '<*>';
CHOICE : '[]';
LINE_COMMENT : '//' ~('\n'|'\r')* '\r'? '\n';
