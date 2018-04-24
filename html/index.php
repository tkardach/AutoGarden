<?php 
	exec('python /cgi-bin/pytest.py', $output, $ret_code);
	
	echo $ret_code;
	echo "Oh hi mark\n"; 
	echo date('H:i:s m/d/y'); 
	phpinfo(); 

?>
