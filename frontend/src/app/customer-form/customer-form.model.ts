import { FormBuilder, FormGroup, Validators } from '@angular/forms';

export function createCustomerForm(fb: FormBuilder): FormGroup {
  return fb.group({
    // Step 1: About Application
    applicationPurpose: ['', Validators.required],
    isSourceOfTruth: ['', Validators.required],
    hasUatEnvironment: ['', Validators.required],
    uatServer: [''],
    uatUsername: [''],
    uatPassword: [''],
    applicationType: ['', Validators.required],
    connectionMethod: ['', Validators.required],
    isLegacyApplication: ['', Validators.required],
    legacyDetails: [''],

    // Step 2: Lifecycle
    lifecycleFeatures: [[] as string[], Validators.required],
    userOnboardingRequired: ['', Validators.required],
    userOnboardingDetails: [''],
    userModificationRequired: ['', Validators.required],
    userModificationDetails: [''],
    userDeletionRequired: ['', Validators.required],
    userDeletionDetails: [''],
    deleteType: [''],
    userReactivationRequired: ['', Validators.required],
    reactivationMethod: [''],
    ssoRequired: ['', Validators.required],
    ssoType: [''],
    reconStrategy: ['', Validators.required],
    defaultEntitlement: [''],
    reconUserTypes: [''],
    entitlementTypes: [''],

    // Step 3: Converged Identity
    ciPackage: ['', Validators.required],
    ciIntegrationRole: ['', Validators.required],
    moduleDiagramNotes: [''],

    // Step 4: Source of Truth
    sotOnboardingStrategy: ['', Validators.required],
    onboardingScan: ['', Validators.required],
    sotAttributes: [[] as string[], Validators.required],
    additionalSotAttributes: [''],

    // Step 5: Encryption
    encryptedFields: [''],
    apiPayloadEncrypted: ['', Validators.required],
    encodedFields: [''],
    encryptionAlgorithm: [''],

    // Step 6: General Information
    apiDocumentationLink: [''],
    specialComments: [''],

    // Step 7: Attachments handled separately
    // Step 8: Review - no fields
  });
}

export const LIFECYCLE_OPTIONS = ['Joiner', 'Mover', 'Leaver', 'SSO', 'Recon'];
export const APPLICATION_TYPES = ['LDAP', 'DB Based', 'API Based', 'SDK Based', 'RPA Based'];
export const SOT_ATTRIBUTES = ['Email Address', 'FullName', 'MobileNumber', 'Employee ID', 'Department'];
export const ALLOWED_FILE_TYPES = [
  '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.csv', '.txt', '.json', '.xml', '.yaml', '.yml',
  '.zip', '.rar', '.7z', '.png', '.jpg', '.jpeg', '.svg', '.gif', '.mp4', '.mov', '.avi',
  '.sql', '.bat', '.sh', '.ps1', '.postman_collection.json',
];

export const WORKSPACE_REQUIRED_KEYS = [
  'applicationPurpose', 'isSourceOfTruth', 'hasUatEnvironment', 'applicationType',
  'connectionMethod', 'isLegacyApplication', 'lifecycleFeatures', 'userOnboardingRequired',
  'userModificationRequired', 'userDeletionRequired', 'userReactivationRequired', 'ssoRequired',
  'reconStrategy', 'ciPackage', 'ciIntegrationRole', 'sotOnboardingStrategy', 'onboardingScan',
  'sotAttributes', 'apiPayloadEncrypted',
];

export const WORKSPACE_SECTIONS = [
  { key: 'about', label: 'About Application', step: 0 },
  { key: 'integration', label: 'Application Integration', step: 1 },
  { key: 'ci', label: 'Converged Identity', step: 2 },
  { key: 'sot', label: 'Source Of Truth', step: 3 },
  { key: 'encryption', label: 'Encryption', step: 4 },
  { key: 'general', label: 'General Information', step: 5 },
  { key: 'comments', label: 'Special Comments', step: 6 },
  { key: 'attachments', label: 'Attachments', step: 7 },
  { key: 'review', label: 'Review & Generate', step: 8 },
];

/** Blue sample hint text from the Application Requirement Gathering document. */
export const FORM_FIELD_HINTS: Record<string, string> = {
  applicationPurpose: "This application's main purpose is to give access to the git",
  isSourceOfTruth: 'This application will be acting a SOT',
  hasUatEnvironment: 'This application has the UAT environment',
  uatServer: 'server: 10.10.10.10',
  uatUsername: 'username: sa',
  uatPassword: 'password: pass@123',
  applicationType: 'So, this application is an api based application. It has restful apis.',
  connectionMethod: 'This application can be connected using API.',
  isLegacyApplication: 'This application is a legacy application.',
  legacyDetails: 'It is a thick client consisting of following exe',
  lifecycleFeatures: 'This application will be having following features to be integrated with CI',
  userOnboardingDetails: 'The user will be created on the application where the email address will be unique',
  userModificationDetails: "The user's personal details and the roles will be changed over the period.",
  userDeletionDetails: 'The user will be deleted on the basis of the email address stored in the DI',
  deleteType: 'The user will be soft deleted from the application',
  reactivationMethod:
    'The application does not require the user to be reactivated / allows users to reactivate themselves / HR permission is required.',
  ssoType: 'This application will require an SSO. We will be using the SAML SSO to connect to this application',
  reconStrategy: 'The recon for this application will be run once.',
  defaultEntitlement: 'Default user role should be assigned to the user.',
  reconUserTypes: 'The active users & the disable users are coming in the same request',
  entitlementTypes: 'The user will be assigned to only one type of entitlement',
  ciPackage: 'The client will be given CI version 10.05.000 and so this application will be integrated on this CI version',
  ciIntegrationRole: 'The application will be integrated as either an SOT or a target application',
  moduleDiagramNotes: 'Below is a sample image (module diagram)',
  sotOnboardingStrategy: 'The application will be using delta on boarding strategy',
  onboardingScan: 'The application will be doing a fresh pull on daily basis at 9pm.',
  sotAttributes: 'The application will be requiring the following attributes from the SOT (Email Address will be used as unique)',
  additionalSotAttributes: 'Add any additional SOT attributes required for this application',
  encryptedFields: 'The user password field is encrypted',
  apiPayloadEncrypted: 'All the apis are encrypted using a standard algorithm',
  encodedFields: 'The user email field is encoded. Also the whole query string is encoded',
  encryptionAlgorithm: 'The application is using a AES encryption mechanism',
  apiDocumentationLink: 'Share the link or add the api documentation here',
  postmanCollection: 'Attach the postman collection of all the apis',
  apiScreenshots: 'Attach the screenshot of the api response',
  specialComments: 'Any special comments required for this application should come here',
};
