//
// ExtSupport.PHP5 - substitute for php5ts.dll
//
// php_mail.h
// - this is slightly modified php_mail.h, originally PHP 5.3.3 source files
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
   | Author: Rasmus Lerdorf <rasmus@lerdorf.on.ca>                        |
   +----------------------------------------------------------------------+
*/

/* $Id: php_mail.h 293036 2010-01-03 09:23:27Z sebastian $ */

#ifndef PHP_MAIL_H
#define PHP_MAIL_H

PHP_FUNCTION(mail);
PHP_MINFO_FUNCTION(mail);

PHP_FUNCTION(ezmlm_hash);
PHPAPI extern int php_mail(char *to, char *subject, char *message, char *headers, char *extra_cmd TSRMLS_DC);

#endif /* PHP_MAIL_H */
