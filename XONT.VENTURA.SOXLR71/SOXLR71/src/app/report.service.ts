import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of, Subject, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private componentMethodCallSource = new Subject<any>();
  componentMethodCalled$ = this.componentMethodCallSource.asObservable();
  // private distributors = [
  //   {
  //     DistributorCode: 'PET1D01 ',
  //     DistributorName: 'W. L. Janaka Jeewantha',
  //   },
  //   {
  //     DistributorCode: 'AKU1D01 ',
  //     DistributorName: 'Dilena Distributors',
  //   },
  //   {
  //     DistributorCode: 'NIT1D01 ',
  //     DistributorName: 'Sanjeewaka Ayurvedic Products (Pvt) Ltd',
  //   },
  //   {
  //     DistributorCode: 'RAG1D01 ',
  //     DistributorName: 'Anura Ferdinandes',
  //   },
  //   {
  //     DistributorCode: 'DEH1D01 ',
  //     DistributorName: 'Onlak Enterprises',
  //   },
  //   {
  //     DistributorCode: 'HOM1D01 ',
  //     DistributorName: 'Onlak Enterprises - Homagama',
  //   },
  //   {
  //     DistributorCode: 'KIR1D01 ',
  //     DistributorName: 'M And M Distributors',
  //   },
  //   {
  //     DistributorCode: 'KAD1D01 ',
  //     DistributorName: 'Coral Auto Lanka (Pvt) Ltd',
  //   },
  //   {
  //     DistributorCode: 'MAH1D01 ',
  //     DistributorName: 'Jayamini Distributors',
  //   },
  //   {
  //     DistributorCode: 'GAL1D01 ',
  //     DistributorName: 'D.L.Sagarika Lakmini',
  //   },
  //   {
  //     DistributorCode: 'KAL1D01 ',
  //     DistributorName: 'New Gamage Stores',
  //   },
  //   {
  //     DistributorCode: 'EMB1D01 ',
  //     DistributorName: 'M.K. Iresha Sadamini - Embilipitiya',
  //   },
  // ];
  // private territories = [
  //   {
  //     TerritoryCode: 'AKU1      ',
  //     TerritoryName: 'Akuressa',
  //   },
  //   {
  //     TerritoryCode: 'ANU1      ',
  //     TerritoryName: 'Anuradhapura 1',
  //   },
  //   {
  //     TerritoryCode: 'ANU2      ',
  //     TerritoryName: 'Anuradhapura 2',
  //   },
  //   {
  //     TerritoryCode: 'AVI1      ',
  //     TerritoryName: 'Awissawella',
  //   },
  //   {
  //     TerritoryCode: 'BAL1      ',
  //     TerritoryName: 'Balangoda',
  //   },
  //   {
  //     TerritoryCode: 'BAN1      ',
  //     TerritoryName: 'Bandarawela',
  //   },
  //   {
  //     TerritoryCode: 'BAT1      ',
  //     TerritoryName: 'Batticaloa',
  //   },
  //   {
  //     TerritoryCode: 'BIB1      ',
  //     TerritoryName: 'Bibila',
  //   },
  //   {
  //     TerritoryCode: 'CHI1      ',
  //     TerritoryName: 'Chilaw 1',
  //   },
  //   {
  //     TerritoryCode: 'CHI2      ',
  //     TerritoryName: 'Chilaw 2',
  //   },
  //   {
  //     TerritoryCode: 'DAM1      ',
  //     TerritoryName: 'Dambulla',
  //   },
  //   {
  //     TerritoryCode: 'DEH1      ',
  //     TerritoryName: 'Dehiwala',
  //   },
  // ];

  constructor(private http: HttpClient) {}

  public getTerritoryPrompt(): Observable<any> {
    // return of(this.territories);
    return this.http
      .get(`${this.getRootURL()}/api/SOXLR71/GetTerritoryPrompt`)
      .pipe(
        map((response: any) => response),
        catchError((error) => this.handleError(error))
      );
  }

  public getDistributorPrompt(): Observable<any> {
    // return of(this.distributors);
    return this.http
      .get(`${this.getRootURL()}/api/SOXLR71/GetDistributorPrompt`)
      .pipe(
        map((response: any) => response),
        catchError((error) => this.handleError(error))
      );
  }

  public generateExcel(data: any): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http
      .post(`${this.getRootURL()}/api/SOXLR71/GenerateExcel`, data, {
        headers,
      })
      .pipe(
        map((response: any) => response),
        catchError((error) => this.handleError(error))
      );
  }

  public getUserName(): Observable<string> {
    return this.http
      .get<string>(`${this.getRootURL()}/api/Message/GetUserName`)
      .pipe(
        catchError((error) => {
          return of('');
        })
      );
  }

  private handleError(error: any): Observable<never> {
    this.componentMethodCallSource.next(error);
    return throwError(() => error);
  }

  public getRootURL = (): string => {
    const { hostname, origin, pathname } = window.location;

    if (hostname === 'localhost') {
      return origin;
    }
    const pathSegments = window.location.pathname
      .split('/')
      .filter((s) => s && s.trim().length > 0);

    const appSegment = pathSegments.length > 0 ? pathSegments[0] : '';

    return appSegment
      ? `${window.location.origin}/${appSegment}`
      : window.location.origin;
  };
}
