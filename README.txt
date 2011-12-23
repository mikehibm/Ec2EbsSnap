===========================================
EBS Snapshot Utility
===========================================

This tool is used for creating and deleting EBS Snapshots on your Amazon EC2 account.

Usage:

  Ec2EbsSnap.exe /L  volume_id                          
    List snapshots for a volume.

  Ec2EbsSnap.exe /c  volume_id                          
    Create a new snapshot for a volume.

  Ec2EbsSnap.exe /d  volume_id description max_generation [max_age] 
    Delete old snapshots as specified in max_generation and max_age parameters.
	You can specify max_age using a number plus 'd', 'h' or 'm' as follows: 
	  14d means 14 days
	  48h means 48 hours
	  90m means 90 minutes

  Ec2EbsSnap.exe /cd volume_id description max_generation [max_age] 
    Create a new snapshot and delete old ones.

  Ec2EbsSnap.exe /enc text_to_encrypt                   
    Encrypt a text to be used in config file.




===========================================
YOU SHOULD EDIT Ec2EbsSnap.exe.config file first in order to access your Amazon EC2 account.

  EncryptedAWSAccessKey = Your ENCRYPTED AWS Access Key
  EncryptedAWSSecretKey = Your ENCRYPTED AWS Secret Key

You can encrypt your keys by executing: 
  Ec2EbsSnap.exe /enc "[Your Access Key]"
  Ec2EbsSnap.exe /enc "[Your Secret Key]"

And then put the results in the Ec2EbsSnap.exe.config file.
===========================================




