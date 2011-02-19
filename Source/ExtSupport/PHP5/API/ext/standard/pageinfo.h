//
// ExtSupport.PHP5 - substitute for php5ts.dll
//
// pageinfo.h
// - this is slightly modified pageinfo.h, originally PHP 5.3.3 source files
//


/*
   +----------------------------------------------------------------------+
   | PHP Version 5                                                        |
   +----------------------------------------------------------------------+
   | Copyright (c) 1997-2010 The PHP Group                                |
   +----------------------------------------------------------------------+
   | This source file is subject to version 3.01 of the PHP license,      |
   | that is bundled with this package in the file LICENSE, and is        |
   | available through the world-wide-web at the following url:           |
   | http://www.php.net/license/3_01.txt                                  |
   | If you did not receive a copy of the PHP license and are unable to   |
   | obtain it through the world-wide-web, please send a note to          |
   | license@php.net so we can mail you a copy immediately.               |
   +----------------------------------------------------------------------+
   | Author: Jim Winstead <jimw@php.net>                                  |
   +----------------------------------------------------------------------+
*/

/* $Id: pageinfo.h 293036 2010-01-03 09:23:27Z sebastian $ */

#ifndef PAGEINFO_H
#define PAGEINFO_H

PHP_FUNCTION(getmyuid);
PHP_FUNCTION(getmygid);
PHP_FUNCTION(getmypid);
PHP_FUNCTION(getmyinode);
PHP_FUNCTION(getlastmod);

PHPAPI void php_statpage(TSRMLS_D);
PHPAPI long php_getlastmod(TSRMLS_D);
extern long php_getuid(void);
extern long php_getgid(void);

#endif
